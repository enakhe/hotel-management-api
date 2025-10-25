using System.ComponentModel.DataAnnotations;
using Azure.Core;
using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Core.Tenant.Commands;
using HotelManagement.Application.Core.Tenant.Queries;
using HotelManagement.Application.Tenant.Queries.GetTenants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace HotelManagement.Web.Controllers.SuperAdmin;

/// <summary>
/// SuperAdmin tenant management controller
/// </summary>
[ApiController]
[Route("cp/tenant")]
[Authorize(Roles = "SuperAdmin")]
public class TenantManagementController(
    ISuperAdminService superAdminService,
    ISender mediator,
    ILogger<TenantManagementController> logger) : ControllerBase
{
    private readonly ISuperAdminService _superAdminService = superAdminService;
    private readonly ILogger<TenantManagementController> _logger = logger;
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Create a new tenant
    /// </summary>
    /// <param name="CreateTenantCommand">Tenant creation request</param>
    /// <returns>Created tenant result</returns>
    [HttpPost]
    public async Task<ActionResult> CreateTenant([FromBody] CreateTenantCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Get list of tenants with filtering and pagination
    /// </summary>
    /// <param name="GetTenantsQuery">List request parameters</param>
    /// <returns>Paginated list of tenants</returns>
    [HttpGet]
    public async Task<ActionResult> GetTenants([FromQuery] GetTenantsQuery request)
    {
        var response = await _mediator.Send(request);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Get detailed information about a specific tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Detailed tenant information</returns>
    [HttpGet("{tenantId}")]
    public async Task<ActionResult> GetTenant(Guid tenantId)
    {
        var request = new GetTenantByIdQuery
        {
            TenantId = tenantId
        };

        var response = await _mediator.Send(request);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Update tenant profile, branding, and contacts
    /// </summary>
    /// <param name="UpdateTenantCommand">Update Tenant Command</param>
    /// <returns>Update result</returns>
    [HttpPatch("update")]
    public async Task<ActionResult> UpdateTenant([FromBody] UpdateTenantCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Lock a tenant (blocks logins and API access)
    /// </summary>
    /// <param name="LockTenantCommand">Lock Tenant Command</param>
    /// <returns>Lock result</returns>
    [HttpPost("lock")]
    public async Task<ActionResult> LockTenant([FromBody] LockTenantCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Unlock a tenant
    /// </summary>
    /// <param name="UnlockTenantCommand">Unlock request with reason</param>
    /// <returns>Unlock result</returns>
    [HttpPost("unlock")]
    public async Task<ActionResult> UnlockTenant([FromBody] UnlockTenantCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Set tenant mode (Active, ReadOnly, Suspended, Locked)
    /// </summary>
    /// <param name="SetTenantModeCommand">Mode change request</param>
    /// <returns>Mode change result</returns>
    [HttpPost("mode")]
    public async Task<ActionResult> SetTenantMode([FromBody] SetTenantModeCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Terminate a tenant (requires step-up authentication)
    /// </summary>
    /// <param name="TerminateTenantCommand">Termination request</param>
    /// <returns>Termination result</returns>
    [HttpPost("terminate")]
    [Authorize(Policy = "RequireStepUpAuth")]
    public async Task<ActionResult> TerminateTenant([FromBody] TerminateTenantCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Export tenant data
    /// </summary>
    /// <param name="ExportTenantDataCommand">Export command</param>
    /// <returns>Export job result</returns>
    [HttpPost("export")]
    public async Task<ActionResult> ExportTenantData([FromBody] ExportTenantDataCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Purge tenant data (requires step-up authentication and 2-man rule)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Purge request</param>
    /// <returns>Purge result</returns>
    [HttpPost("purge")]
    [Authorize(Policy = "RequireStepUpAuth")]
    public async Task<ActionResult> PurgeTenantData([FromBody] PurgeTenantDataCommand command)
    {
        var response = await _mediator.Send(command);

        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Get tenant usage statistics
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Usage statistics</returns>
    [HttpGet("usage/{tenantId}")]
    public async Task<ActionResult> GetTenantUsage(Guid tenantId)
    {
        var request = new GetTenantHealthCommand
        {
            TenantId = tenantId
        };

        var response = await _mediator.Send(request);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }

    /// <summary>
    /// Get tenant health status
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Health status</returns>
    [HttpGet("health/{tenantId}")]
    public async Task<ActionResult> GetTenantHealth(Guid tenantId)
    {
        var request = new GetTenantHealthCommand
        {
            TenantId = tenantId
        };

        var response = await _mediator.Send(request);
        return !response.Succeeded ? StatusCode(response.StatusCode, response) : (ActionResult)Ok(response);
    }
}
