using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for managing tenant subscriptions
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Create a new subscription for a tenant
    /// </summary>
    Task<Result<SubscriptionResponseDto>> CreateSubscriptionAsync(
        CreateSubscriptionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subscription by ID
    /// </summary>
    Task<Result<SubscriptionResponseDto>> GetSubscriptionByIdAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subscription for a specific tenant
    /// </summary>
    Task<Result<SubscriptionResponseDto>> GetSubscriptionByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all subscriptions (SuperAdmin only)
    /// </summary>
    Task<Result<PaginatedResult<SubscriptionResponseDto>>> GetSubscriptionsAsync(
        GetSubscriptionsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a subscription
    /// </summary>
    Task<Result<SubscriptionResponseDto>> UpdateSubscriptionAsync(
        Guid subscriptionId,
        UpdateSubscriptionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a subscription
    /// </summary>
    Task<Result<bool>> CancelSubscriptionAsync(
        Guid subscriptionId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suspend a subscription (for non-payment)
    /// </summary>
    Task<Result<bool>> SuspendSubscriptionAsync(
        Guid subscriptionId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reactivate a suspended subscription
    /// </summary>
    Task<Result<bool>> ReactivateSubscriptionAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active subscriptions
    /// </summary>
    Task<Result<List<SubscriptionResponseDto>>> GetActiveSubscriptionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subscriptions due for renewal
    /// </summary>
    Task<Result<List<SubscriptionResponseDto>>> GetSubscriptionsDueForRenewalAsync(
        DateTime date,
        CancellationToken cancellationToken = default);
}


