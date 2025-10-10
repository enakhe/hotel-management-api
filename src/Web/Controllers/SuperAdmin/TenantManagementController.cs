using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Web.Controllers.SuperAdmin;

/// <summary>
/// SuperAdmin tenant management controller
/// </summary>
[ApiController]
[Route("cp/tenant-management")]
[Authorize(Roles = "SuperAdmin")]
public class TenantManagementController(
    ISuperAdminService superAdminService,
    ISuperAdminAuditService auditService,
    ILogger<TenantManagementController> logger) : ControllerBase
{
    private readonly ISuperAdminService _superAdminService = superAdminService;
    private readonly ISuperAdminAuditService _auditService = auditService;
    private readonly ILogger<TenantManagementController> _logger = logger;

    /// <summary>
    /// Create a new tenant
    /// </summary>
    /// <param name="request">Tenant creation request</param>
    /// <returns>Created tenant result</returns>
    [HttpPost]
    public async Task<ActionResult<CreateTenantResult>> CreateTenant([FromBody] CreateTenantRequest request)
    {
        try
        {
            var result = await _superAdminService.CreateTenantAsync(request);

            return !result.Success ? (ActionResult<CreateTenantResult>)BadRequest(result) : (ActionResult<CreateTenantResult>)CreatedAtAction(nameof(GetTenant), new { tenantId = result.TenantId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant");
            return StatusCode(500, "An error occurred while creating the tenant");
        }
    }

    /// <summary>
    /// Get list of tenants with filtering and pagination
    /// </summary>
    /// <param name="request">List request parameters</param>
    /// <returns>Paginated list of tenants</returns>
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<TenantSummary>>> GetTenants([FromQuery] TenantListRequest request)
    {
        try
        {
            var result = await _superAdminService.GetTenantsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenants list");
            return StatusCode(500, "An error occurred while retrieving tenants");
        }
    }

    /// <summary>
    /// Get detailed information about a specific tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Detailed tenant information</returns>
    [HttpGet("{tenantId}")]
    public async Task<ActionResult<TenantDetail>> GetTenant(Guid tenantId)
    {
        try
        {
            var tenant = await _superAdminService.GetTenantDetailAsync(tenantId);

            if (tenant == null)
            {
                return NotFound($"Tenant with ID {tenantId} not found");
            }

            return Ok(tenant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred while retrieving the tenant");
        }
    }

    /// <summary>
    /// Update tenant profile, branding, and contacts
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Update result</returns>
    [HttpPatch("{tenantId}")]
    public async Task<ActionResult> UpdateTenant(Guid tenantId, [FromBody] UpdateTenantRequest request)
    {
        try
        {
            var success = await _superAdminService.UpdateTenantAsync(tenantId, request);

            if (!success)
            {
                return NotFound($"Tenant with ID {tenantId} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred while updating the tenant");
        }
    }

    /// <summary>
    /// Lock a tenant (blocks logins and API access)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Lock request with reason</param>
    /// <returns>Lock result</returns>
    [HttpPost("{tenantId}/lock")]
    public async Task<ActionResult> LockTenant(Guid tenantId, [FromBody] LockTenantRequest request)
    {
        try
        {
            var success = await _superAdminService.LockTenantAsync(tenantId, request.Reason);

            if (!success)
            {
                return NotFound($"Tenant with ID {tenantId} not found");
            }

            return Ok(new { Message = "Tenant locked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred while locking the tenant");
        }
    }

    /// <summary>
    /// Unlock a tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Unlock request with reason</param>
    /// <returns>Unlock result</returns>
    [HttpPost("{tenantId}/unlock")]
    public async Task<ActionResult> UnlockTenant(Guid tenantId, [FromBody] UnlockTenantRequest request)
    {
        try
        {
            var success = await _superAdminService.UnlockTenantAsync(tenantId, request.Reason);

            if (!success)
            {
                return NotFound($"Tenant with ID {tenantId} not found");
            }

            return Ok(new { Message = "Tenant unlocked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred while unlocking the tenant");
        }
    }

    /// <summary>
    /// Set tenant mode (Active, ReadOnly, Suspended, Locked)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Mode change request</param>
    /// <returns>Mode change result</returns>
    [HttpPost("{tenantId}/mode")]
    public async Task<ActionResult> SetTenantMode(Guid tenantId, [FromBody] SetTenantModeRequest request)
    {
        try
        {
            var success = await _superAdminService.SetTenantModeAsync(tenantId, request.Mode, request.Reason);

            if (!success)
            {
                return NotFound($"Tenant with ID {tenantId} not found");
            }

            return Ok(new { Message = $"Tenant mode set to {request.Mode} successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting tenant mode for {TenantId}", tenantId);
            return StatusCode(500, "An error occurred while setting tenant mode");
        }
    }

    /// <summary>
    /// Terminate a tenant (requires step-up authentication)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Termination request</param>
    /// <returns>Termination result</returns>
    [HttpPost("{tenantId}/terminate")]
    [Authorize(Policy = "RequireStepUpAuth")]
    public async Task<ActionResult> TerminateTenant(Guid tenantId, [FromBody] TerminateTenantRequest request)
    {
        try
        {
            var success = await _superAdminService.TerminateTenantAsync(tenantId, request.Reason, request.EffectiveDate);

            if (!success)
            {
                return NotFound($"Tenant with ID {tenantId} not found");
            }

            return Ok(new { Message = "Tenant termination scheduled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred while terminating the tenant");
        }
    }

    /// <summary>
    /// Export tenant data
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Export request</param>
    /// <returns>Export job result</returns>
    [HttpPost("{tenantId}/export")]
    public async Task<ActionResult<ExportJobResult>> ExportTenantData(Guid tenantId, [FromBody] ExportTenantDataRequest request)
    {
        try
        {
            var result = await _superAdminService.ExportTenantDataAsync(tenantId, request.Options);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting export for tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred while starting the export");
        }
    }

    /// <summary>
    /// Purge tenant data (requires step-up authentication and 2-man rule)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Purge request</param>
    /// <returns>Purge result</returns>
    [HttpPost("{tenantId}/purge")]
    [Authorize(Policy = "RequireStepUpAuth")]
    public async Task<ActionResult> PurgeTenantData(Guid tenantId, [FromBody] PurgeTenantDataRequest request)
    {
        try
        {
            var success = await _superAdminService.PurgeTenantDataAsync(tenantId, request.Reason);

            if (!success)
            {
                return NotFound($"Tenant with ID {tenantId} not found");
            }

            return Ok(new { Message = "Tenant data purge completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error purging data for tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred while purging tenant data");
        }
    }

    /// <summary>
    /// Get tenant usage statistics
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Usage statistics</returns>
    [HttpGet("{tenantId}/usage")]
    public async Task<ActionResult<TenantUsage>> GetTenantUsage(Guid tenantId)
    {
        try
        {
            var usage = await _superAdminService.GetTenantUsageAsync(tenantId);

            if (usage == null)
            {
                return NotFound($"Tenant with ID {tenantId} not found");
            }

            return Ok(usage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage for tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred while retrieving tenant usage");
        }
    }

    /// <summary>
    /// Get tenant health status
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <returns>Health status</returns>
    [HttpGet("{tenantId}/health")]
    public async Task<ActionResult<TenantHealth>> GetTenantHealth(Guid tenantId)
    {
        try
        {
            var health = await _superAdminService.GetTenantHealthAsync(tenantId);

            if (health == null)
            {
                return NotFound($"Tenant with ID {tenantId} not found");
            }

            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health for tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred while retrieving tenant health");
        }
    }
}
