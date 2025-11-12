using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for tracking and managing usage metrics
/// </summary>
public interface IUsageTrackingService
{
    /// <summary>
    /// Record a usage event
    /// </summary>
    Task<Result<bool>> RecordUsageAsync(
        Guid tenantId,
        UsageMetric metric,
        decimal quantity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get usage summary for a tenant in a billing period
    /// </summary>
    Task<Result<UsageSummaryDto>> GetUsageSummaryAsync(
        Guid tenantId,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get usage for a specific metric
    /// </summary>
    Task<Result<decimal>> GetUsageByMetricAsync(
        Guid tenantId,
        UsageMetric metric,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate overage costs for a subscription's billing period
    /// </summary>
    Task<Result<OverageCalculationDto>> CalculateOveragesAsync(
        Guid subscriptionId,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if tenant is within their usage limits
    /// </summary>
    Task<Result<LimitCheckDto>> CheckLimitAsync(
        Guid tenantId,
        UsageMetric metric,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aggregate usage records into summary for billing period
    /// </summary>
    Task<Result<UsageAggregation>> AggregateUsageAsync(
        Guid subscriptionId,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get usage aggregation for a subscription
    /// </summary>
    Task<Result<UsageAggregationDto>> GetUsageAggregationAsync(
        Guid subscriptionId,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset usage counters (for new billing period)
    /// </summary>
    Task<Result<bool>> ResetUsageCountersAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}

