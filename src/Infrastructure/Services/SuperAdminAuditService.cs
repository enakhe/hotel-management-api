using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities.SuperAdmin;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services;

/// <summary>
/// Service for auditing SuperAdmin actions
/// </summary>
public class SuperAdminAuditService : ISuperAdminAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SuperAdminAuditService> _logger;

    public SuperAdminAuditService(ApplicationDbContext context, ILogger<SuperAdminAuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogActionAsync(
        string action,
        string targetType,
        string? targetId = null,
        Guid? tenantId = null,
        string? details = null,
        string? changes = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            var auditLog = new SuperAdminAuditLogEntity
            {
                Id = Guid.NewGuid(),
                SuperAdminId = Guid.NewGuid(), // This should come from context
                Username = "Unknown", // This should come from context
                Action = action,
                TargetType = targetType,
                TargetId = targetId,
                TenantId = tenantId,
                Details = details,
                Changes = changes,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            _context.SuperAdminAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging SuperAdmin action: {Action}", action);
        }
    }

    public async Task<PaginatedResult<SuperAdminAuditLog>> GetAuditLogsAsync(
        Guid superAdminId,
        int page = 1,
        int size = 20,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        try
        {
            var query = _context.SuperAdminAuditLogs
                .AsNoTracking()
                .Where(log => log.SuperAdminId == superAdminId);

            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp <= toDate.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(log => log.Timestamp)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(log => new SuperAdminAuditLog
                {
                    Id = log.Id,
                    SuperAdminId = log.SuperAdminId,
                    Username = log.Username,
                    Action = log.Action,
                    TargetType = log.TargetType,
                    TargetId = log.TargetId,
                    TenantId = log.TenantId,
                    Details = log.Details,
                    Changes = log.Changes,
                    IpAddress = log.IpAddress,
                    UserAgent = log.UserAgent,
                    Timestamp = log.Timestamp
                })
                .ToListAsync();

            return new PaginatedResult<SuperAdminAuditLog>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                Size = size
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for SuperAdmin {SuperAdminId}", superAdminId);
            return new PaginatedResult<SuperAdminAuditLog>();
        }
    }

    public async Task<PaginatedResult<SuperAdminAuditLog>> GetTenantAuditLogsAsync(
        Guid tenantId,
        int page = 1,
        int size = 20)
    {
        try
        {
            var query = _context.SuperAdminAuditLogs
                .AsNoTracking()
                .Where(log => log.TenantId == tenantId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(log => log.Timestamp)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(log => new SuperAdminAuditLog
                {
                    Id = log.Id,
                    SuperAdminId = log.SuperAdminId,
                    Username = log.Username,
                    Action = log.Action,
                    TargetType = log.TargetType,
                    TargetId = log.TargetId,
                    TenantId = log.TenantId,
                    Details = log.Details,
                    Changes = log.Changes,
                    IpAddress = log.IpAddress,
                    UserAgent = log.UserAgent,
                    Timestamp = log.Timestamp
                })
                .ToListAsync();

            return new PaginatedResult<SuperAdminAuditLog>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                Size = size
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for tenant {TenantId}", tenantId);
            return new PaginatedResult<SuperAdminAuditLog>();
        }
    }
}
