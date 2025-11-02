using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for managing report subscriptions
/// </summary>
public interface IReportSubscriptionService
{
    /// <summary>
    /// Create a new subscription
    /// </summary>
    Task<Result<ReportSubscriptionDto>> CreateSubscriptionAsync(CreateReportSubscriptionDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a subscription
    /// </summary>
    Task<Result<bool>> DeleteSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subscriptions with filtering
    /// </summary>
    Task<Result<List<ReportSubscriptionDto>>> GetSubscriptionsAsync(Guid? reportScheduleId = null, Guid? superAdminId = null, bool? isActive = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get subscription by ID
    /// </summary>
    Task<Result<ReportSubscriptionDto>> GetSubscriptionByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update subscription sent count
    /// </summary>
    Task UpdateSentCountAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}

