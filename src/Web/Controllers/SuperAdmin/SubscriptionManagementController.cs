using Asp.Versioning;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Common.Security;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.SuperAdmin;

/// <summary>
/// SuperAdmin subscription management controller
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("cp/api/v{version:apiVersion}/subscriptions")]
[Authorize(Roles = "SuperAdministrator")]
[Produces("application/json")]
public class SubscriptionManagementController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionManagementController> _logger;

    public SubscriptionManagementController(
        ISubscriptionService subscriptionService,
        ILogger<SubscriptionManagementController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all subscriptions
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<SubscriptionResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptions([FromQuery] GetSubscriptionsRequest request)
    {
        var result = await _subscriptionService.GetSubscriptionsAsync(request);
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Get subscription by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscription(Guid id)
    {
        var result = await _subscriptionService.GetSubscriptionByIdAsync(id);
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Get subscription by tenant ID
    /// </summary>
    [HttpGet("tenant/{tenantId}")]
    [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscriptionByTenant(Guid tenantId)
    {
        var result = await _subscriptionService.GetSubscriptionByTenantAsync(tenantId);
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Create a new subscription
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
    {
        var result = await _subscriptionService.CreateSubscriptionAsync(request);
        
        if (result.Succeeded)
        {
            return CreatedAtAction(
                nameof(GetSubscription),
                new { id = result.Data!.Id },
                result.Data);
        }

        return StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Update a subscription
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSubscription(Guid id, [FromBody] UpdateSubscriptionRequest request)
    {
        var result = await _subscriptionService.UpdateSubscriptionAsync(id, request);
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Cancel a subscription
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSubscription(Guid id, [FromBody] CancelSubscriptionRequest request)
    {
        var result = await _subscriptionService.CancelSubscriptionAsync(id, request.Reason);
        return result.Succeeded ? Ok(new { success = true }) : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Suspend a subscription
    /// </summary>
    [HttpPost("{id}/suspend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendSubscription(Guid id, [FromBody] SuspendSubscriptionRequest request)
    {
        var result = await _subscriptionService.SuspendSubscriptionAsync(id, request.Reason);
        return result.Succeeded ? Ok(new { success = true }) : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Reactivate a subscription
    /// </summary>
    [HttpPost("{id}/reactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateSubscription(Guid id)
    {
        var result = await _subscriptionService.ReactivateSubscriptionAsync(id);
        return result.Succeeded ? Ok(new { success = true }) : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Get all active subscriptions
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<SubscriptionResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveSubscriptions()
    {
        var result = await _subscriptionService.GetActiveSubscriptionsAsync();
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Get subscriptions due for renewal
    /// </summary>
    [HttpGet("due-for-renewal")]
    [ProducesResponseType(typeof(List<SubscriptionResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionsDueForRenewal([FromQuery] DateTime? date)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var result = await _subscriptionService.GetSubscriptionsDueForRenewalAsync(targetDate);
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, result.Errors);
    }
}

/// <summary>
/// Request to cancel subscription
/// </summary>
public record CancelSubscriptionRequest
{
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Request to suspend subscription
/// </summary>
public record SuspendSubscriptionRequest
{
    public string Reason { get; init; } = string.Empty;
}

