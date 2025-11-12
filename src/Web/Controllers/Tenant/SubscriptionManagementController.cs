using Asp.Versioning;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Common.Security;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.Tenant;

/// <summary>
/// Tenant subscription management controller (self-service)
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/subscription")]
[Authorize(Roles = "Administrator")]
[Produces("application/json")]
public class SubscriptionManagementController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IPricingService _pricingService;
    private readonly IPlanService _planService;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<SubscriptionManagementController> _logger;

    public SubscriptionManagementController(
        ISubscriptionService subscriptionService,
        IPricingService pricingService,
        IPlanService planService,
        ITenantContext tenantContext,
        ILogger<SubscriptionManagementController> logger)
    {
        _subscriptionService = subscriptionService;
        _pricingService = pricingService;
        _planService = planService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Get available plans
    /// </summary>
    [HttpGet("plans")]
    [ProducesResponseType(typeof(PaginatedResult<PlanResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailablePlans([FromQuery] PlanListRequest request)
    {
        // Only show active plans
        request = request with { IsActive = true };
        var result = await _planService.GetPlansAsync(request);
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, string.Join(", ", result.Errors));
    }

    /// <summary>
    /// Get price quote for a plan
    /// </summary>
    [HttpGet("plans/{planId}/quote")]
    [ProducesResponseType(typeof(PriceQuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPriceQuote(Guid planId)
    {
        if (!_tenantContext.TenantId.HasValue)
            return BadRequest("Tenant context not resolved");

        var result = await _pricingService.GetPriceQuoteAsync(_tenantContext.TenantId.Value, planId);
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, string.Join(", ", result.Errors));
    }

    /// <summary>
    /// Request plan upgrade (generates quote with proration)
    /// </summary>
    [HttpPost("upgrade")]
    [ProducesResponseType(typeof(ProrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestUpgrade([FromBody] UpgradePlanRequest request)
    {
        if (!_tenantContext.TenantId.HasValue)
            return BadRequest("Tenant context not resolved");

        // Get current subscription
        var subscriptionResult = await _subscriptionService.GetSubscriptionByTenantAsync(_tenantContext.TenantId.Value);
        if (!subscriptionResult.Succeeded || subscriptionResult.Data == null)
            return StatusCode(subscriptionResult.StatusCode, string.Join(", ", subscriptionResult.Errors));

        var subscription = subscriptionResult.Data;

        // Calculate proration
        var prorationResult = await _pricingService.CalculateProrationAsync(
            subscription.Id,
            request.NewPlanId,
            request.ChangeDate ?? DateTime.UtcNow);

        if (!prorationResult.Succeeded)
            return StatusCode(prorationResult.StatusCode, string.Join(", ", prorationResult.Errors));

        return Ok(prorationResult.Data);
    }

    /// <summary>
    /// Cancel subscription
    /// </summary>
    [HttpPost("cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSubscription([FromBody] CancelSubscriptionRequest request)
    {
        if (!_tenantContext.TenantId.HasValue)
            return BadRequest("Tenant context not resolved");

        var subscriptionResult = await _subscriptionService.GetSubscriptionByTenantAsync(_tenantContext.TenantId.Value);
        if (!subscriptionResult.Succeeded || subscriptionResult.Data == null)
            return StatusCode(subscriptionResult.StatusCode, string.Join(", ", subscriptionResult.Errors));

        var result = await _subscriptionService.CancelSubscriptionAsync(
            subscriptionResult.Data.Id,
            request.Reason ?? "Requested by tenant");

        return result.Succeeded ? Ok(new { success = true }) : StatusCode(result.StatusCode, string.Join(", ", result.Errors));
    }
}

/// <summary>
/// Request to upgrade plan
/// </summary>
public record UpgradePlanRequest
{
    public Guid NewPlanId { get; init; }
    public DateTime? ChangeDate { get; init; }
}

/// <summary>
/// Request to cancel subscription
/// </summary>
public record CancelSubscriptionRequest
{
    public string? Reason { get; init; }
}

