using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.SuperAdmin;

/// <summary>
/// SuperAdmin audit controller
/// </summary>
[ApiController]
[Route("cp/audit")]
[Authorize(Roles = "SuperAdmin")]
public class AuditController : ControllerBase
{
    private readonly ISuperAdminAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(ISuperAdminAuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get audit logs for a specific SuperAdmin
    /// </summary>
    /// <param name="superAdminId">SuperAdmin ID</param>
    /// <param name="request">Audit log request parameters</param>
    /// <returns>Paginated audit logs</returns>
    [HttpGet("superadmin/{superAdminId}")]
    public async Task<ActionResult<PaginatedResult<SuperAdminAuditLog>>> GetSuperAdminAuditLogs(
        Guid superAdminId,
        [FromQuery] GetAuditLogsRequest request)
    {
        try
        {
            var result = await _auditService.GetAuditLogsAsync(
                superAdminId,
                request.Page,
                request.Size,
                request.FromDate,
                request.ToDate);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for SuperAdmin {SuperAdminId}", superAdminId);
            return StatusCode(500, "An error occurred while retrieving audit logs");
        }
    }

    /// <summary>
    /// Get audit logs for a specific tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">Audit log request parameters</param>
    /// <returns>Paginated audit logs</returns>
    [HttpGet("tenant/{tenantId}")]
    public async Task<ActionResult<PaginatedResult<SuperAdminAuditLog>>> GetTenantAuditLogs(
        Guid tenantId,
        [FromQuery] GetTenantAuditLogsRequest request)
    {
        try
        {
            var result = await _auditService.GetTenantAuditLogsAsync(
                tenantId,
                request.Page,
                request.Size);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for tenant {TenantId}", tenantId);
            return StatusCode(500, "An error occurred while retrieving audit logs");
        }
    }

    /// <summary>
    /// Get audit logs for all SuperAdmin actions
    /// </summary>
    /// <param name="request">Audit log request parameters</param>
    /// <returns>Paginated audit logs</returns>
    [HttpGet("all")]
    public Task<ActionResult<PaginatedResult<SuperAdminAuditLog>>> GetAllAuditLogs(
        [FromQuery] GetAllAuditLogsRequest request)
    {
        try
        {
            // This would need to be implemented in the audit service
            // For now, return empty result
            var result = new PaginatedResult<SuperAdminAuditLog>
            {
                Items = Enumerable.Empty<SuperAdminAuditLog>(),
                TotalCount = 0,
                Page = request.Page,
                Size = request.Size
            };

            return Task.FromResult<ActionResult<PaginatedResult<SuperAdminAuditLog>>>(Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all audit logs");
            return Task.FromResult<ActionResult<PaginatedResult<SuperAdminAuditLog>>>(StatusCode(500, "An error occurred while retrieving audit logs"));
        }
    }
}

// Request DTOs
public record GetAuditLogsRequest(
    int Page = 1,
    int Size = 20,
    DateTime? FromDate = null,
    DateTime? ToDate = null);

public record GetTenantAuditLogsRequest(
    int Page = 1,
    int Size = 20);

public record GetAllAuditLogsRequest(
    int Page = 1,
    int Size = 20,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Action = null,
    string? TargetType = null);
