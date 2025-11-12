using AutoMapper;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Billing;

public class SubscriptionService : ISubscriptionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly IMapper _mapper;
    private readonly IPricingService _pricingService;

    public SubscriptionService(
        ApplicationDbContext context,
        ILogger<SubscriptionService> logger,
        IMapper mapper,
        IPricingService pricingService)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
        _pricingService = pricingService;
    }

    public async Task<Result<SubscriptionResponseDto>> CreateSubscriptionAsync(
        CreateSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate tenant exists
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken);

            if (tenant == null)
                return Result<SubscriptionResponseDto>.Failure("Tenant not found", 404);

            // Validate plan exists
            var plan = await _context.Plans
                .Include(p => p.PlanModules)
                    .ThenInclude(pm => pm.Module)
                        .ThenInclude(m => m.Pricing)
                .FirstOrDefaultAsync(p => p.Id == request.PlanId, cancellationToken);

            if (plan == null)
                return Result<SubscriptionResponseDto>.Failure("Plan not found", 404);

            if (!plan.IsActive)
                return Result<SubscriptionResponseDto>.Failure("Plan is not active", 400);

            // Check if tenant already has an active subscription
            var existingSubscription = await _context.Subscriptions
                .Where(s => s.TenantId == request.TenantId)
                .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingSubscription != null)
                return Result<SubscriptionResponseDto>.Failure("Tenant already has an active subscription", 400);

            // Calculate monthly price
            var priceResult = await _pricingService.CalculatePlanPriceAsync(request.PlanId, cancellationToken);
            if (!priceResult.Succeeded)
                return Result<SubscriptionResponseDto>.Failure(priceResult.Errors, priceResult.StatusCode);

            var monthlyPrice = priceResult.Data;

            // Apply custom discount if provided
            if (request.CustomDiscount.HasValue && request.CustomDiscount.Value > 0)
            {
                monthlyPrice -= monthlyPrice * (request.CustomDiscount.Value / 100);
            }

            // Generate subscription number
            var subscriptionCount = await _context.Subscriptions.CountAsync(cancellationToken);
            var subscriptionNumber = $"SUB-{DateTime.UtcNow.Year}-{(subscriptionCount + 1):D4}";

            // Calculate next billing date
            var nextBillingDate = request.TrialEndDate ?? request.StartDate.AddMonths(1);

            // Create subscription
            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                SubscriptionNumber = subscriptionNumber,
                TenantId = request.TenantId,
                PlanId = request.PlanId,
                StartDate = request.StartDate,
                TrialEndDate = request.TrialEndDate,
                Status = request.TrialEndDate.HasValue ? SubscriptionStatus.Trial : SubscriptionStatus.PendingPayment,
                BillingCycle = request.BillingCycle,
                MonthlyPrice = monthlyPrice,
                Currency = plan.Currency,
                CustomDiscount = request.CustomDiscount,
                DiscountReason = request.DiscountReason,
                NextBillingDate = nextBillingDate,
                AutoRenew = request.AutoRenew,
                Notes = request.Notes
            };

            _context.Subscriptions.Add(subscription);

            // Create subscription modules (lock in module prices)
            foreach (var planModule in plan.PlanModules)
            {
                var modulePrice = planModule.Module?.Pricing?.Price ?? 0;

                var subscriptionModule = new SubscriptionModule
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscription.Id,
                    ModuleId = planModule.ModuleId,
                    Price = modulePrice,
                    IsOptional = !planModule.IsRequired,
                    AddedAt = DateTime.UtcNow
                };

                _context.SubscriptionModules.Add(subscriptionModule);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created subscription {SubscriptionNumber} for tenant {TenantId}", 
                subscriptionNumber, request.TenantId);

            // Return created subscription
            return await GetSubscriptionByIdAsync(subscription.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription for tenant {TenantId}", request.TenantId);
            return Result<SubscriptionResponseDto>.Failure("An error occurred while creating subscription", 500);
        }
    }

    public async Task<Result<SubscriptionResponseDto>> GetSubscriptionByIdAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Tenant)
                .Include(s => s.Plan)
                .Include(s => s.SubscriptionModules)
                    .ThenInclude(sm => sm.Module)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);

            if (subscription == null)
                return Result<SubscriptionResponseDto>.Failure("Subscription not found", 404);

            var dto = _mapper.Map<SubscriptionResponseDto>(subscription);

            return Result<SubscriptionResponseDto>.Success(dto, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription {SubscriptionId}", subscriptionId);
            return Result<SubscriptionResponseDto>.Failure("An error occurred while getting subscription", 500);
        }
    }

    public async Task<Result<SubscriptionResponseDto>> GetSubscriptionByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Tenant)
                .Include(s => s.Plan)
                .Include(s => s.SubscriptionModules)
                    .ThenInclude(sm => sm.Module)
                .AsNoTracking()
                .Where(s => s.TenantId == tenantId)
                .OrderByDescending(s => s.StartDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (subscription == null)
                return Result<SubscriptionResponseDto>.Failure("No subscription found for tenant", 404);

            var dto = _mapper.Map<SubscriptionResponseDto>(subscription);

            return Result<SubscriptionResponseDto>.Success(dto, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription for tenant {TenantId}", tenantId);
            return Result<SubscriptionResponseDto>.Failure("An error occurred while getting subscription", 500);
        }
    }

    public async Task<Result<PaginatedResult<SubscriptionResponseDto>>> GetSubscriptionsAsync(
        GetSubscriptionsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Subscriptions
                .Include(s => s.Tenant)
                .Include(s => s.Plan)
                .Include(s => s.SubscriptionModules)
                    .ThenInclude(sm => sm.Module)
                .AsNoTracking();

            // Apply filters
            if (request.TenantId.HasValue)
                query = query.Where(s => s.TenantId == request.TenantId.Value);

            if (request.Status.HasValue)
                query = query.Where(s => s.Status == request.Status.Value);

            if (request.PlanId.HasValue)
                query = query.Where(s => s.PlanId == request.PlanId.Value);

            if (request.StartDateFrom.HasValue)
                query = query.Where(s => s.StartDate >= request.StartDateFrom.Value);

            if (request.StartDateTo.HasValue)
                query = query.Where(s => s.StartDate <= request.StartDateTo.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(s => 
                    s.SubscriptionNumber.Contains(request.SearchTerm) ||
                    s.Tenant.Name.Contains(request.SearchTerm));
            }

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = request.SortBy.ToLowerInvariant() switch
            {
                "subscriptionnumber" => request.SortDescending 
                    ? query.OrderByDescending(s => s.SubscriptionNumber)
                    : query.OrderBy(s => s.SubscriptionNumber),
                "startdate" => request.SortDescending
                    ? query.OrderByDescending(s => s.StartDate)
                    : query.OrderBy(s => s.StartDate),
                "status" => request.SortDescending
                    ? query.OrderByDescending(s => s.Status)
                    : query.OrderBy(s => s.Status),
                "monthlyprice" => request.SortDescending
                    ? query.OrderByDescending(s => s.MonthlyPrice)
                    : query.OrderBy(s => s.MonthlyPrice),
                _ => request.SortDescending
                    ? query.OrderByDescending(s => s.StartDate)
                    : query.OrderBy(s => s.StartDate)
            };

            // Apply pagination
            var subscriptions = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<SubscriptionResponseDto>>(subscriptions);

            var result = new PaginatedResult<SubscriptionResponseDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = request.Page,
                Size = request.PageSize
            };

            return Result<PaginatedResult<SubscriptionResponseDto>>.Success(result, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscriptions");
            return Result<PaginatedResult<SubscriptionResponseDto>>.Failure("An error occurred while getting subscriptions", 500);
        }
    }

    public async Task<Result<SubscriptionResponseDto>> UpdateSubscriptionAsync(
        Guid subscriptionId,
        UpdateSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);

            if (subscription == null)
                return Result<SubscriptionResponseDto>.Failure("Subscription not found", 404);

            // Update fields
            if (request.NextBillingDate.HasValue)
                subscription.NextBillingDate = request.NextBillingDate.Value;

            if (request.AutoRenew.HasValue)
                subscription.AutoRenew = request.AutoRenew.Value;

            if (request.CustomDiscount.HasValue)
            {
                subscription.CustomDiscount = request.CustomDiscount.Value;
                subscription.DiscountReason = request.DiscountReason;
            }

            if (request.Notes != null)
                subscription.Notes = request.Notes;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated subscription {SubscriptionId}", subscriptionId);

            return await GetSubscriptionByIdAsync(subscriptionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription {SubscriptionId}", subscriptionId);
            return Result<SubscriptionResponseDto>.Failure("An error occurred while updating subscription", 500);
        }
    }

    public async Task<Result<bool>> CancelSubscriptionAsync(
        Guid subscriptionId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);

            if (subscription == null)
                return Result<bool>.Failure("Subscription not found", 404);

            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.EndDate = DateTime.UtcNow;
            subscription.Notes = $"Cancelled: {reason}. {subscription.Notes}";

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cancelled subscription {SubscriptionId}. Reason: {Reason}", 
                subscriptionId, reason);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription {SubscriptionId}", subscriptionId);
            return Result<bool>.Failure("An error occurred while cancelling subscription", 500);
        }
    }

    public async Task<Result<bool>> SuspendSubscriptionAsync(
        Guid subscriptionId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);

            if (subscription == null)
                return Result<bool>.Failure("Subscription not found", 404);

            subscription.Status = SubscriptionStatus.Suspended;
            subscription.Notes = $"Suspended: {reason}. {subscription.Notes}";

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Suspended subscription {SubscriptionId}. Reason: {Reason}",
                subscriptionId, reason);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending subscription {SubscriptionId}", subscriptionId);
            return Result<bool>.Failure("An error occurred while suspending subscription", 500);
        }
    }

    public async Task<Result<bool>> ReactivateSubscriptionAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);

            if (subscription == null)
                return Result<bool>.Failure("Subscription not found", 404);

            subscription.Status = SubscriptionStatus.Active;
            subscription.Notes = $"Reactivated on {DateTime.UtcNow:yyyy-MM-dd}. {subscription.Notes}";

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reactivated subscription {SubscriptionId}", subscriptionId);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating subscription {SubscriptionId}", subscriptionId);
            return Result<bool>.Failure("An error occurred while reactivating subscription", 500);
        }
    }

    public async Task<Result<List<SubscriptionResponseDto>>> GetActiveSubscriptionsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptions = await _context.Subscriptions
                .Include(s => s.Tenant)
                .Include(s => s.Plan)
                .Include(s => s.SubscriptionModules)
                    .ThenInclude(sm => sm.Module)
                .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<SubscriptionResponseDto>>(subscriptions);

            return Result<List<SubscriptionResponseDto>>.Success(dtos, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active subscriptions");
            return Result<List<SubscriptionResponseDto>>.Failure("An error occurred while getting active subscriptions", 500);
        }
    }

    public async Task<Result<List<SubscriptionResponseDto>>> GetSubscriptionsDueForRenewalAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptions = await _context.Subscriptions
                .Include(s => s.Tenant)
                .Include(s => s.Plan)
                .Include(s => s.SubscriptionModules)
                    .ThenInclude(sm => sm.Module)
                .Where(s => s.Status == SubscriptionStatus.Active)
                .Where(s => s.NextBillingDate.Date == date.Date)
                .Where(s => s.AutoRenew)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<SubscriptionResponseDto>>(subscriptions);

            _logger.LogInformation("Found {Count} subscriptions due for renewal on {Date}",
                dtos.Count, date.Date);

            return Result<List<SubscriptionResponseDto>>.Success(dtos, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscriptions due for renewal");
            return Result<List<SubscriptionResponseDto>>.Failure("An error occurred while getting subscriptions", 500);
        }
    }
}

