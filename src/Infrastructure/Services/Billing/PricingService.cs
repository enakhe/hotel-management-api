using AutoMapper;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Billing;

public class PricingService : IPricingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PricingService> _logger;
    private readonly IMapper _mapper;
    private const decimal DEFAULT_TAX_RATE = 7.5m; // 7.5% VAT for Nigeria

    public PricingService(
        ApplicationDbContext context,
        ILogger<PricingService> logger,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<decimal>> CalculatePlanPriceAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        try
        {
            var plan = await _context.Plans
                .Include(p => p.PlanModules)
                    .ThenInclude(pm => pm.Module)
                        .ThenInclude(m => m.Pricing)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

            if (plan == null)
                return Result<decimal>.Failure("Plan not found", 404);

            if (!plan.IsActive)
                return Result<decimal>.Failure("Plan is not active", 400);

            decimal totalPrice = 0;

            // Calculate based on pricing strategy
            switch (plan.PricingStrategy)
            {
                case PlanPricingStrategy.Fixed:
                    totalPrice = plan.BasePrice;
                    break;

                case PlanPricingStrategy.ModuleSum:
                case PlanPricingStrategy.ModuleSumWithMarkup:
                    // Sum all module prices
                    foreach (var planModule in plan.PlanModules)
                    {
                        if (planModule.Module?.Pricing != null)
                        {
                            totalPrice += planModule.Module.Pricing.Price ?? 0;
                        }
                    }

                    // Apply markup if configured
                    if (plan.PricingStrategy == PlanPricingStrategy.ModuleSumWithMarkup)
                    {
                        totalPrice *= 1.10m; // 10% markup
                    }
                    break;
            }

            // Apply plan-level discount
            if (plan.DiscountPercentage.HasValue && plan.DiscountPercentage.Value > 0)
            {
                var discountAmount = totalPrice * (plan.DiscountPercentage.Value / 100);
                totalPrice -= discountAmount;
            }

            _logger.LogInformation("Calculated price for plan {PlanId}: {Price}", planId, totalPrice);

            return Result<decimal>.Success(totalPrice, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating plan price for plan {PlanId}", planId);
            return Result<decimal>.Failure("An error occurred while calculating plan price", 500);
        }
    }

    public async Task<Result<PriceQuoteDto>> GetPriceQuoteAsync(Guid tenantId, Guid planId, CancellationToken cancellationToken = default)
    {
        try
        {
            var plan = await _context.Plans
                .Include(p => p.PlanModules)
                    .ThenInclude(pm => pm.Module)
                        .ThenInclude(m => m.Pricing)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

            if (plan == null)
                return Result<PriceQuoteDto>.Failure("Plan not found", 404);

            // Calculate monthly price
            var priceResult = await CalculatePlanPriceAsync(planId, cancellationToken);
            if (!priceResult.Succeeded)
                return Result<PriceQuoteDto>.Failure(priceResult.Errors, priceResult.StatusCode);

            var monthlyPrice = priceResult.Data;

            // Calculate tax
            var taxAmount = monthlyPrice * (DEFAULT_TAX_RATE / 100);

            // Calculate total first month (includes setup fee if any)
            var totalFirstMonth = monthlyPrice + (plan.SetupFee ?? 0) + taxAmount;
            var totalRecurring = monthlyPrice + taxAmount;

            // Get module details
            var modules = plan.PlanModules
                .Where(pm => pm.Module != null)
                .Select(pm => new ModulePriceDto
                {
                    ModuleId = pm.ModuleId,
                    ModuleName = pm.Module!.Name,
                    Price = pm.Module.Pricing?.Price ?? 0,
                    IsRequired = pm.IsRequired
                })
                .ToList();

            var quote = new PriceQuoteDto
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
                MonthlyPrice = monthlyPrice,
                SetupFee = plan.SetupFee,
                Discount = plan.DiscountPercentage,
                TaxRate = DEFAULT_TAX_RATE,
                TaxAmount = taxAmount,
                TotalFirstMonth = totalFirstMonth,
                TotalRecurring = totalRecurring,
                Currency = plan.Currency,
                Modules = modules,
                ValidUntil = DateTime.UtcNow.AddDays(30)
            };

            return Result<PriceQuoteDto>.Success(quote, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price quote for plan {PlanId}", planId);
            return Result<PriceQuoteDto>.Failure("An error occurred while getting price quote", 500);
        }
    }

    public async Task<Result<ProrationDto>> CalculateProrationAsync(
        Guid subscriptionId,
        Guid newPlanId,
        DateTime changeDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);

            if (subscription == null)
                return Result<ProrationDto>.Failure("Subscription not found", 404);

            var newPlan = await _context.Plans.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == newPlanId, cancellationToken);

            if (newPlan == null)
                return Result<ProrationDto>.Failure("New plan not found", 404);

            // Calculate new plan price
            var newPlanPriceResult = await CalculatePlanPriceAsync(newPlanId, cancellationToken);
            if (!newPlanPriceResult.Succeeded)
                return Result<ProrationDto>.Failure(newPlanPriceResult.Errors, newPlanPriceResult.StatusCode);

            var currentPlanPrice = subscription.MonthlyPrice;
            var newPlanPrice = newPlanPriceResult.Data;

            // Calculate days remaining in current billing cycle
            var lastBillingDate = subscription.LastBillingDate ?? subscription.StartDate;
            var daysInCycle = (subscription.NextBillingDate - lastBillingDate).Days;
            var daysRemaining = (subscription.NextBillingDate - changeDate).Days;

            if (daysRemaining < 0)
                daysRemaining = 0;

            // Calculate unused amount from current plan
            var unusedAmount = (currentPlanPrice / daysInCycle) * daysRemaining;

            // Calculate prorated amount for new plan
            var newPlanProrata = (newPlanPrice / daysInCycle) * daysRemaining;

            // Calculate amount due or credit
            var difference = newPlanProrata - unusedAmount;
            var amountDue = difference > 0 ? difference : 0;
            var creditAmount = difference < 0 ? Math.Abs(difference) : 0;

            var proration = new ProrationDto
            {
                CurrentPlanId = subscription.PlanId,
                CurrentPlanName = subscription.Plan.Name,
                CurrentPlanPrice = currentPlanPrice,
                NewPlanId = newPlanId,
                NewPlanName = newPlan.Name,
                NewPlanPrice = newPlanPrice,
                ChangeDate = changeDate,
                DaysRemaining = daysRemaining,
                UnusedAmount = unusedAmount,
                NewPlanProrata = newPlanProrata,
                AmountDue = amountDue,
                CreditAmount = creditAmount,
                IsUpgrade = newPlanPrice > currentPlanPrice,
                Currency = subscription.Currency
            };

            return Result<ProrationDto>.Success(proration, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating proration for subscription {SubscriptionId}", subscriptionId);
            return Result<ProrationDto>.Failure("An error occurred while calculating proration", 500);
        }
    }

    public async Task<Result<PriceBreakdownDto>> GetPriceBreakdownAsync(Guid planId, CancellationToken cancellationToken = default)
    {
        try
        {
            var plan = await _context.Plans
                .Include(p => p.PlanModules)
                    .ThenInclude(pm => pm.Module)
                        .ThenInclude(m => m.Pricing)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

            if (plan == null)
                return Result<PriceBreakdownDto>.Failure("Plan not found", 404);

            // Get module details and sum
            var modules = plan.PlanModules
                .Where(pm => pm.Module?.Pricing != null)
                .Select(pm => new ModulePriceDto
                {
                    ModuleId = pm.ModuleId,
                    ModuleName = pm.Module!.Name,
                    Price = pm.Module.Pricing?.Price ?? 0,
                    IsRequired = pm.IsRequired
                })
                .ToList();

            var modulesTotal = modules.Sum(m => m.Price);

            // Calculate base price based on strategy
            decimal basePrice = plan.PricingStrategy == PlanPricingStrategy.Fixed
                ? plan.BasePrice
                : modulesTotal;

            // Apply markup if needed
            if (plan.PricingStrategy == PlanPricingStrategy.ModuleSumWithMarkup)
            {
                basePrice *= 1.10m;
            }

            // Calculate discount
            decimal? discountAmount = null;
            if (plan.DiscountPercentage.HasValue && plan.DiscountPercentage.Value > 0)
            {
                discountAmount = basePrice * (plan.DiscountPercentage.Value / 100);
            }

            var subtotal = basePrice - (discountAmount ?? 0);
            var taxAmount = subtotal * (DEFAULT_TAX_RATE / 100);
            var total = subtotal + taxAmount;

            var breakdown = new PriceBreakdownDto
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
                BasePrice = basePrice,
                ModulesTotal = modulesTotal,
                SetupFee = plan.SetupFee,
                DiscountAmount = discountAmount,
                DiscountPercentage = plan.DiscountPercentage,
                SubtotalBeforeTax = subtotal,
                TaxAmount = taxAmount,
                Total = total,
                Currency = plan.Currency,
                Modules = modules
            };

            return Result<PriceBreakdownDto>.Success(breakdown, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price breakdown for plan {PlanId}", planId);
            return Result<PriceBreakdownDto>.Failure("An error occurred while getting price breakdown", 500);
        }
    }
}


