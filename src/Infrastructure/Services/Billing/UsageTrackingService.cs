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

public class UsageTrackingService : IUsageTrackingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UsageTrackingService> _logger;
    private readonly IMapper _mapper;
    private readonly ITenantContext _tenantContext;

    // Default overage pricing (can be overridden in database)
    private const decimal EMAIL_OVERAGE_PRICE = 0.01m; // ₦0.01 per email
    private const decimal SMS_OVERAGE_PRICE = 26.25m; // ₦26.25 per SMS
    private const decimal STORAGE_OVERAGE_PRICE = 0.375m; // ₦0.375 per GB
    private const decimal API_OVERAGE_PRICE = 0.0375m; // ₦0.0375 per 100 calls

    public UsageTrackingService(
        ApplicationDbContext context,
        ILogger<UsageTrackingService> logger,
        IMapper mapper,
        ITenantContext tenantContext)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> RecordUsageAsync(
        Guid tenantId,
        UsageMetric metric,
        decimal quantity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get active subscription
            var subscription = await _context.Subscriptions
                .Where(s => s.TenantId == tenantId)
                .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial)
                .FirstOrDefaultAsync(cancellationToken);

            if (subscription == null)
            {
                _logger.LogWarning("No active subscription found for tenant {TenantId}", tenantId);
                return Result<bool>.Success(false, 200);
            }

            // Determine billing period
            var periodStart = subscription.LastBillingDate ?? subscription.StartDate;
            var periodEnd = subscription.NextBillingDate;

            var usageRecord = new UsageRecord
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                SubscriptionId = subscription.Id,
                Metric = metric,
                Quantity = quantity,
                RecordedAt = DateTime.UtcNow,
                BillingPeriodStart = periodStart,
                BillingPeriodEnd = periodEnd,
                IsBilled = false
            };

            _context.UsageRecords.Add(usageRecord);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Recorded usage for tenant {TenantId}: {Metric} = {Quantity}",
                tenantId, metric, quantity);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording usage for tenant {TenantId}", tenantId);
            return Result<bool>.Failure("An error occurred while recording usage", 500);
        }
    }

    public async Task<Result<UsageSummaryDto>> GetUsageSummaryAsync(
        Guid tenantId,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await _context.Tenants
                .Include(t => t.Plan)
                    .ThenInclude(p => p.Limits)
                .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

            if (tenant == null)
                return Result<UsageSummaryDto>.Failure("Tenant not found", 404);

            var limits = tenant.Plan.Limits;

            // Get usage records for period
            var usageRecords = await _context.UsageRecords
                .Where(ur => ur.TenantId == tenantId)
                .Where(ur => ur.RecordedAt >= periodStart && ur.RecordedAt < periodEnd)
                .GroupBy(ur => ur.Metric)
                .Select(g => new { Metric = g.Key, Total = g.Sum(ur => ur.Quantity) })
                .ToListAsync(cancellationToken);

            // Email usage
            var emailsSent = (int)(usageRecords.FirstOrDefault(ur => ur.Metric == UsageMetric.EmailsSent)?.Total ?? 0);
            var emailsIncluded = limits.MaxEmailsPerMonth == -1 ? int.MaxValue : limits.MaxEmailsPerMonth;
            var emailsOverage = Math.Max(0, emailsSent - emailsIncluded);
            var emailOverageCost = emailsOverage * EMAIL_OVERAGE_PRICE;

            // SMS usage
            var smsSent = (int)(usageRecords.FirstOrDefault(ur => ur.Metric == UsageMetric.SmsSent)?.Total ?? 0);
            var smsIncluded = limits.MaxSmsPerMonth == -1 ? int.MaxValue : limits.MaxSmsPerMonth;
            var smsOverage = Math.Max(0, smsSent - smsIncluded);
            var smsOverageCost = smsOverage * SMS_OVERAGE_PRICE;

            // Storage usage (get latest)
            var storageUsed = usageRecords.FirstOrDefault(ur => ur.Metric == UsageMetric.StorageGB)?.Total ?? 0;
            var storageIncluded = limits.MaxStorageGB == -1 ? decimal.MaxValue : limits.MaxStorageGB;
            var storageOverage = Math.Max(0, storageUsed - storageIncluded);
            var storageOverageCost = storageOverage * STORAGE_OVERAGE_PRICE;

            // API usage
            var apiCalls = (int)(usageRecords.FirstOrDefault(ur => ur.Metric == UsageMetric.ApiCalls)?.Total ?? 0);
            var apiIncluded = limits.ApiRateLimit == -1 ? int.MaxValue : limits.ApiRateLimit * 730; // Assume hourly rate * avg hours/month
            var apiOverage = Math.Max(0, apiCalls - apiIncluded);
            var apiOverageCost = (apiOverage / 100m) * API_OVERAGE_PRICE;

            var summary = new UsageSummaryDto
            {
                TenantId = tenantId,
                TenantName = tenant.Name,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                EmailsSent = emailsSent,
                EmailsIncluded = emailsIncluded == int.MaxValue ? -1 : emailsIncluded,
                EmailsOverage = emailsOverage,
                EmailOverageCost = emailOverageCost,
                SmsSent = smsSent,
                SmsIncluded = smsIncluded == int.MaxValue ? -1 : smsIncluded,
                SmsOverage = smsOverage,
                SmsOverageCost = smsOverageCost,
                StorageUsedGB = storageUsed,
                StorageIncludedGB = storageIncluded == decimal.MaxValue ? -1 : storageIncluded,
                StorageOverageGB = storageOverage,
                StorageOverageCost = storageOverageCost,
                ApiCallsMade = apiCalls,
                ApiCallsIncluded = apiIncluded == int.MaxValue ? -1 : apiIncluded,
                ApiCallsOverage = apiOverage,
                ApiCallsOverageCost = apiOverageCost,
                TotalOverageCost = emailOverageCost + smsOverageCost + storageOverageCost + apiOverageCost,
                Currency = tenant.CurrencyCode ?? "NGN"
            };

            return Result<UsageSummaryDto>.Success(summary, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage summary for tenant {TenantId}", tenantId);
            return Result<UsageSummaryDto>.Failure("An error occurred while getting usage summary", 500);
        }
    }

    public async Task<Result<decimal>> GetUsageByMetricAsync(
        Guid tenantId,
        UsageMetric metric,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var total = await _context.UsageRecords
                .Where(ur => ur.TenantId == tenantId)
                .Where(ur => ur.Metric == metric)
                .Where(ur => ur.RecordedAt >= periodStart && ur.RecordedAt < periodEnd)
                .SumAsync(ur => ur.Quantity, cancellationToken);

            return Result<decimal>.Success(total, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage by metric for tenant {TenantId}", tenantId);
            return Result<decimal>.Failure("An error occurred while getting usage", 500);
        }
    }

    public async Task<Result<OverageCalculationDto>> CalculateOveragesAsync(
        Guid subscriptionId,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Tenant)
                .Include(s => s.Plan)
                    .ThenInclude(p => p.Limits)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);

            if (subscription == null)
                return Result<OverageCalculationDto>.Failure("Subscription not found", 404);

            var usageSummary = await GetUsageSummaryAsync(subscription.TenantId, periodStart, periodEnd, cancellationToken);
            if (!usageSummary.Succeeded || usageSummary.Data == null)
                return Result<OverageCalculationDto>.Failure(usageSummary.Errors, usageSummary.StatusCode);

            var usage = usageSummary.Data;
            var overageItems = new List<OverageItemDto>();

            // Email overages
            if (usage.EmailsOverage > 0)
            {
                overageItems.Add(new OverageItemDto
                {
                    Metric = UsageMetric.EmailsSent,
                    MetricDisplay = "Emails",
                    QuantityUsed = usage.EmailsSent,
                    QuantityIncluded = usage.EmailsIncluded,
                    QuantityOverage = usage.EmailsOverage,
                    PricePerUnit = EMAIL_OVERAGE_PRICE,
                    TotalCost = usage.EmailOverageCost
                });
            }

            // SMS overages
            if (usage.SmsOverage > 0)
            {
                overageItems.Add(new OverageItemDto
                {
                    Metric = UsageMetric.SmsSent,
                    MetricDisplay = "SMS Messages",
                    QuantityUsed = usage.SmsSent,
                    QuantityIncluded = usage.SmsIncluded,
                    QuantityOverage = usage.SmsOverage,
                    PricePerUnit = SMS_OVERAGE_PRICE,
                    TotalCost = usage.SmsOverageCost
                });
            }

            // Storage overages
            if (usage.StorageOverageGB > 0)
            {
                overageItems.Add(new OverageItemDto
                {
                    Metric = UsageMetric.StorageGB,
                    MetricDisplay = "Storage (GB)",
                    QuantityUsed = usage.StorageUsedGB,
                    QuantityIncluded = usage.StorageIncludedGB,
                    QuantityOverage = usage.StorageOverageGB,
                    PricePerUnit = STORAGE_OVERAGE_PRICE,
                    TotalCost = usage.StorageOverageCost
                });
            }

            // API overages
            if (usage.ApiCallsOverage > 0)
            {
                overageItems.Add(new OverageItemDto
                {
                    Metric = UsageMetric.ApiCalls,
                    MetricDisplay = "API Calls",
                    QuantityUsed = usage.ApiCallsMade,
                    QuantityIncluded = usage.ApiCallsIncluded,
                    QuantityOverage = usage.ApiCallsOverage,
                    PricePerUnit = API_OVERAGE_PRICE,
                    TotalCost = usage.ApiCallsOverageCost
                });
            }

            var calculation = new OverageCalculationDto
            {
                SubscriptionId = subscriptionId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                HasOverages = overageItems.Any(),
                TotalOverageCost = usage.TotalOverageCost,
                OverageItems = overageItems,
                Currency = subscription.Currency
            };

            return Result<OverageCalculationDto>.Success(calculation, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating overages for subscription {SubscriptionId}", subscriptionId);
            return Result<OverageCalculationDto>.Failure("An error occurred while calculating overages", 500);
        }
    }

    public async Task<Result<LimitCheckDto>> CheckLimitAsync(
        Guid tenantId,
        UsageMetric metric,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                    .ThenInclude(p => p.Limits)
                .Where(s => s.TenantId == tenantId)
                .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial)
                .FirstOrDefaultAsync(cancellationToken);

            if (subscription == null)
                return Result<LimitCheckDto>.Failure("No active subscription found", 404);

            var limits = subscription.Plan.Limits;
            var periodStart = subscription.LastBillingDate ?? subscription.StartDate;
            var periodEnd = subscription.NextBillingDate;

            var currentUsage = await GetUsageByMetricAsync(tenantId, metric, periodStart, periodEnd, cancellationToken);
            if (!currentUsage.Succeeded)
                return Result<LimitCheckDto>.Failure(currentUsage.Errors, currentUsage.StatusCode);

            var used = currentUsage.Data;
            decimal limit = metric switch
            {
                UsageMetric.EmailsSent => limits.MaxEmailsPerMonth == -1 ? decimal.MaxValue : limits.MaxEmailsPerMonth,
                UsageMetric.SmsSent => limits.MaxSmsPerMonth == -1 ? decimal.MaxValue : limits.MaxSmsPerMonth,
                UsageMetric.StorageGB => limits.MaxStorageGB == -1 ? decimal.MaxValue : limits.MaxStorageGB,
                UsageMetric.ApiCalls => limits.ApiRateLimit == -1 ? decimal.MaxValue : limits.ApiRateLimit * 730,
                UsageMetric.Users => limits.MaxUsers == -1 ? decimal.MaxValue : limits.MaxUsers,
                UsageMetric.Bookings => limits.MaxBookings == -1 ? decimal.MaxValue : limits.MaxBookings,
                _ => decimal.MaxValue
            };

            var remaining = limit == decimal.MaxValue ? decimal.MaxValue : Math.Max(0, limit - used);
            var percentageUsed = limit == decimal.MaxValue ? 0 : (used / limit) * 100;
            var isOverLimit = used > limit && limit != decimal.MaxValue;
            var isNearLimit = percentageUsed >= 80 && !isOverLimit;

            var check = new LimitCheckDto
            {
                Metric = metric,
                MetricDisplay = metric.ToString(),
                CurrentUsage = used,
                Limit = limit == decimal.MaxValue ? -1 : limit,
                Remaining = remaining == decimal.MaxValue ? -1 : remaining,
                PercentageUsed = percentageUsed,
                IsOverLimit = isOverLimit,
                IsNearLimit = isNearLimit
            };

            return Result<LimitCheckDto>.Success(check, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking limit for tenant {TenantId}", tenantId);
            return Result<LimitCheckDto>.Failure("An error occurred while checking limit", 500);
        }
    }

    public async Task<Result<UsageAggregation>> AggregateUsageAsync(
        Guid subscriptionId,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                    .ThenInclude(p => p.Limits)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);

            if (subscription == null)
                return Result<UsageAggregation>.Failure("Subscription not found", 404);

            var limits = subscription.Plan.Limits;

            // Check if aggregation already exists
            var existing = await _context.UsageAggregations
                .FirstOrDefaultAsync(ua =>
                    ua.SubscriptionId == subscriptionId &&
                    ua.BillingPeriodStart == periodStart &&
                    ua.BillingPeriodEnd == periodEnd,
                    cancellationToken);

            if (existing != null && existing.IsFinalized)
            {
                _logger.LogDebug("Usage already aggregated for subscription {SubscriptionId}", subscriptionId);
                return Result<UsageAggregation>.Success(existing, 200);
            }

            var usageSummary = await GetUsageSummaryAsync(subscription.TenantId, periodStart, periodEnd, cancellationToken);
            if (!usageSummary.Succeeded || usageSummary.Data == null)
                return Result<UsageAggregation>.Failure(usageSummary.Errors, usageSummary.StatusCode);

            var usage = usageSummary.Data;

            var aggregation = existing ?? new UsageAggregation
            {
                Id = Guid.NewGuid(),
                TenantId = subscription.TenantId,
                SubscriptionId = subscriptionId,
                BillingPeriodStart = periodStart,
                BillingPeriodEnd = periodEnd
            };

            // Update aggregation
            aggregation.EmailsSent = usage.EmailsSent;
            aggregation.EmailsIncluded = usage.EmailsIncluded;
            aggregation.EmailsOverage = usage.EmailsOverage;
            aggregation.EmailOverageCost = usage.EmailOverageCost;

            aggregation.SmsSent = usage.SmsSent;
            aggregation.SmsIncluded = usage.SmsIncluded;
            aggregation.SmsOverage = usage.SmsOverage;
            aggregation.SmsOverageCost = usage.SmsOverageCost;

            aggregation.StorageUsedGB = usage.StorageUsedGB;
            aggregation.StorageIncludedGB = usage.StorageIncludedGB;
            aggregation.StorageOverageGB = usage.StorageOverageGB;
            aggregation.StorageOverageCost = usage.StorageOverageCost;

            aggregation.ApiCallsMade = usage.ApiCallsMade;
            aggregation.ApiCallsIncluded = usage.ApiCallsIncluded;
            aggregation.ApiCallsOverage = usage.ApiCallsOverage;
            aggregation.ApiCallsOverageCost = usage.ApiCallsOverageCost;

            aggregation.TotalOverageCost = usage.TotalOverageCost;
            aggregation.IsFinalized = true;
            aggregation.FinalizedAt = DateTime.UtcNow;

            if (existing == null)
                _context.UsageAggregations.Add(aggregation);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Aggregated usage for subscription {SubscriptionId}. Total overage cost: {Cost}",
                subscriptionId, aggregation.TotalOverageCost);

            return Result<UsageAggregation>.Success(aggregation, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating usage for subscription {SubscriptionId}", subscriptionId);
            return Result<UsageAggregation>.Failure("An error occurred while aggregating usage", 500);
        }
    }

    public async Task<Result<UsageAggregationDto>> GetUsageAggregationAsync(
        Guid subscriptionId,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var aggregation = await _context.UsageAggregations
                .AsNoTracking()
                .FirstOrDefaultAsync(ua =>
                    ua.SubscriptionId == subscriptionId &&
                    ua.BillingPeriodStart == periodStart &&
                    ua.BillingPeriodEnd == periodEnd,
                    cancellationToken);

            if (aggregation == null)
            {
                // Create on-the-fly if not exists
                var result = await AggregateUsageAsync(subscriptionId, periodStart, periodEnd, cancellationToken);
                if (!result.Succeeded || result.Data == null)
                    return Result<UsageAggregationDto>.Failure(result.Errors, result.StatusCode);

                aggregation = result.Data;
            }

            var dto = _mapper.Map<UsageAggregationDto>(aggregation);
            return Result<UsageAggregationDto>.Success(dto, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage aggregation");
            return Result<UsageAggregationDto>.Failure("An error occurred while getting usage aggregation", 500);
        }
    }

    public async Task<Result<bool>> ResetUsageCountersAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // This is typically called at the start of a new billing period
            // Usage records are kept for historical purposes but marked as billed
            var unbilledRecords = await _context.UsageRecords
                .Where(ur => ur.TenantId == tenantId)
                .Where(ur => !ur.IsBilled)
                .ToListAsync(cancellationToken);

            foreach (var record in unbilledRecords)
            {
                record.IsBilled = true;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reset usage counters for tenant {TenantId}. {Count} records marked as billed",
                tenantId, unbilledRecords.Count);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting usage counters for tenant {TenantId}", tenantId);
            return Result<bool>.Failure("An error occurred while resetting usage counters", 500);
        }
    }
}

