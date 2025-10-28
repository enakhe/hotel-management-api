using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for auditing SuperAdmin actions
/// </summary>
public interface ISuperAdminAuditService
{
    /// <summary>
    /// Logs a SuperAdmin action with full context
    /// </summary>
    /// <param name="action">The action performed</param>
    /// <param name="targetType">Type of target (e.g., "Tenant", "User")</param>
    /// <param name="targetId">ID of the target</param>
    /// <param name="tenantId">Tenant ID if applicable</param>
    /// <param name="details">Additional details</param>
    /// <param name="changes">JSON string of changes made</param>
    /// <param name="ipAddress">IP address of the request</param>
    /// <param name="userAgent">User agent of the request</param>
    Task LogActionAsync(
        string action,
        string targetType,
        string? targetId = null,
        Guid? tenantId = null,
        string? details = null,
        string? changes = null,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Gets audit logs for a specific SuperAdmin
    /// </summary>
    /// <param name="superAdminId">SuperAdmin ID</param>
    /// <param name="page">Page number</param>
    /// <param name="size">Page size</param>
    /// <param name="fromDate">Filter from date</param>
    /// <param name="toDate">Filter to date</param>
    /// <returns>Paginated audit logs</returns>
    Task<PaginatedResult<SuperAdminAuditLog>> GetAuditLogsAsync(
        Guid superAdminId,
        int page = 1,
        int size = 20,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    /// <summary>
    /// Gets audit logs for a specific tenant
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="page">Page number</param>
    /// <param name="size">Page size</param>
    /// <returns>Paginated audit logs</returns>
    Task<PaginatedResult<SuperAdminAuditLog>> GetTenantAuditLogsAsync(
        Guid tenantId,
        int page = 1,
        int size = 20);
}

/// <summary>
/// SuperAdmin audit log entry
/// </summary>
public record SuperAdminAuditLog
{
    public Guid Id { get; init; }
    public Guid SuperAdminId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string TargetType { get; init; } = string.Empty;
    public string? TargetId { get; init; }
    public Guid? TenantId { get; init; }
    public string? Details { get; init; }
    public string? Changes { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public DateTime Timestamp { get; init; }
}

