using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for calculating prices, quotes, and proration
/// </summary>
public interface IPricingService
{
    /// <summary>
    /// Calculate the total monthly price for a plan
    /// </summary>
    Task<Result<decimal>> CalculatePlanPriceAsync(Guid planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a detailed price quote for a plan subscription
    /// </summary>
    Task<Result<PriceQuoteDto>> GetPriceQuoteAsync(Guid tenantId, Guid planId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate proration amount for plan upgrade/downgrade
    /// </summary>
    Task<Result<ProrationDto>> CalculateProrationAsync(
        Guid subscriptionId,
        Guid newPlanId,
        DateTime changeDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate the price breakdown for a subscription
    /// </summary>
    Task<Result<PriceBreakdownDto>> GetPriceBreakdownAsync(Guid planId, CancellationToken cancellationToken = default);
}


