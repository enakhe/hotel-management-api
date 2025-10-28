using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Core.LicenseManagement.Commands;
using HotelManagement.Application.Core.LicenseManagement.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.SuperAdmin;

/// <summary>
/// SuperAdmin license management controller
/// </summary>
[ApiController]
[Route("cp/licenses")]
[Authorize(Roles = "SuperAdmin")]
public class LicenseManagementController(
    ISuperAdminService superAdminService,
    ISender mediator,
    ILogger<LicenseManagementController> logger) : ControllerBase
{
    private readonly ISuperAdminService _superAdminService = superAdminService;
    private readonly ILogger<LicenseManagementController> _logger = logger;
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Get list of licenses with filtering and pagination
    /// </summary>
    /// <param name="request">List request parameters</param>
    /// <returns>Paginated list of licenses</returns>
    [HttpGet]
    public async Task<ActionResult> GetLicenses([FromQuery] GetLicensesQuery request)
    {
        var response = await _mediator.Send(request);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Create a new license
    /// </summary>
    /// <param name="command">License creation request</param>
    /// <returns>Created license result</returns>
    [HttpPost]
    public async Task<ActionResult> CreateLicense([FromBody] CreateLicenseCommand command)
    {
        var response = await _mediator.Send(command);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Get a specific license by ID
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <returns>License details</returns>
    [HttpGet("{licenseId}")]
    public async Task<ActionResult> GetLicenseById(Guid licenseId)
    {
        var query = new GetLicenseByIdQuery { LicenseId = licenseId };
        var response = await _mediator.Send(query);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Get a specific license by license key
    /// </summary>
    /// <param name="licenseKey">License key</param>
    /// <returns>License details</returns>
    [HttpGet("key/{licenseKey}")]
    public async Task<ActionResult> GetLicenseByKey(string licenseKey)
    {
        var query = new GetLicenseByKeyQuery { LicenseKey = licenseKey };
        var response = await _mediator.Send(query);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Update an existing license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="command">License update request</param>
    /// <returns>Updated license result</returns>
    [HttpPatch("{licenseId}")]
    public async Task<ActionResult> UpdateLicense(Guid licenseId, [FromBody] UpdateLicenseCommand command)
    {
        var updateCommand = command with { LicenseId = licenseId };
        var response = await _mediator.Send(updateCommand);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Delete a license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <returns>Deletion result</returns>
    [HttpDelete("{licenseId}")]
    public async Task<ActionResult> DeleteLicense(Guid licenseId)
    {
        var command = new DeleteLicenseCommand { LicenseId = licenseId };
        var response = await _mediator.Send(command);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Validate a license key
    /// </summary>
    /// <param name="command">License validation request</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    public async Task<ActionResult> ValidateLicense([FromBody] ValidateLicenseCommand command)
    {
        var response = await _mediator.Send(command);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Validate license key format
    /// </summary>
    /// <param name="command">Format validation request</param>
    /// <returns>Format validation result</returns>
    [HttpPost("validate-format")]
    public async Task<ActionResult> ValidateLicenseKeyFormat([FromBody] ValidateLicenseKeyFormatCommand command)
    {
        var response = await _mediator.Send(command);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Generate a license key
    /// </summary>
    /// <param name="command">Key generation request</param>
    /// <returns>Generated license key</returns>
    [HttpPost("generate-key")]
    public async Task<ActionResult> GenerateLicenseKey([FromBody] GenerateLicenseKeyCommand command)
    {
        var response = await _mediator.Send(command);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Renew a license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="command">Renewal request</param>
    /// <returns>Renewed license result</returns>
    [HttpPost("{licenseId}/renew")]
    public async Task<ActionResult> RenewLicense(Guid licenseId, [FromBody] RenewLicenseCommand command)
    {
        var renewalCommand = command with { LicenseId = licenseId };
        var response = await _mediator.Send(renewalCommand);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Transfer a license to another tenant
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="command">Transfer request</param>
    /// <returns>Transferred license result</returns>
    [HttpPost("{licenseId}/transfer")]
    public async Task<ActionResult> TransferLicense(Guid licenseId, [FromBody] TransferLicenseCommand command)
    {
        var transferCommand = command with { LicenseId = licenseId };
        var response = await _mediator.Send(transferCommand);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Suspend a license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="command">Suspension request</param>
    /// <returns>Suspended license result</returns>
    [HttpPost("{licenseId}/suspend")]
    public async Task<ActionResult> SuspendLicense(Guid licenseId, [FromBody] SuspendLicenseCommand command)
    {
        var suspendCommand = command with { LicenseId = licenseId };
        var response = await _mediator.Send(suspendCommand);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Revoke a license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="command">Revocation request</param>
    /// <returns>Revoked license result</returns>
    [HttpPost("{licenseId}/revoke")]
    public async Task<ActionResult> RevokeLicense(Guid licenseId, [FromBody] RevokeLicenseCommand command)
    {
        var revokeCommand = command with { LicenseId = licenseId };
        var response = await _mediator.Send(revokeCommand);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Activate a license
    /// </summary>
    /// <param name="licenseId">License ID</param>
    /// <param name="command">Activation request</param>
    /// <returns>Activated license result</returns>
    [HttpPost("{licenseId}/activate")]
    public async Task<ActionResult> ActivateLicense(Guid licenseId, [FromBody] ActivateLicenseCommand command)
    {
        var activateCommand = command with { LicenseId = licenseId };
        var response = await _mediator.Send(activateCommand);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Get license analytics
    /// </summary>
    /// <returns>License analytics data</returns>
    [HttpGet("analytics")]
    public async Task<ActionResult> GetLicenseAnalytics()
    {
        var query = new GetLicenseAnalyticsQuery();
        var response = await _mediator.Send(query);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Get license usage data
    /// </summary>
    /// <returns>License usage data</returns>
    [HttpGet("usage")]
    public async Task<ActionResult> GetLicenseUsage()
    {
        var query = new GetLicenseUsageQuery();
        var response = await _mediator.Send(query);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Get licenses expiring within specified days
    /// </summary>
    /// <param name="days">Number of days</param>
    /// <returns>Expiring licenses</returns>
    [HttpGet("expiring/{days}")]
    public async Task<ActionResult> GetExpiringLicenses(int days)
    {
        var query = new GetExpiringLicensesQuery { Days = days };
        var response = await _mediator.Send(query);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Bulk update multiple licenses
    /// </summary>
    /// <param name="updates">Bulk update requests</param>
    /// <returns>Bulk update result</returns>
    [HttpPatch("bulk")]
    public async Task<ActionResult> BulkUpdateLicenses([FromBody] BulkLicenseUpdateRequest[] updates)
    {
        var response = await _superAdminService.BulkUpdateLicensesAsync(updates);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

}
