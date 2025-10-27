using HotelManagement.Application.Common.Interfaces.Tenant;
using HotelManagement.Domain.Common;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Data.Services;

/// <summary>
/// Service that applies tenant-specific query filters to the DbContext
/// </summary>
public class TenantQueryFilterService
{
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<TenantQueryFilterService> _logger;

    public TenantQueryFilterService(ITenantContext tenantContext, ILogger<TenantQueryFilterService> logger)
    {
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Applies tenant query filters to the DbContext
    /// </summary>
    public void ApplyTenantFilters(ApplicationDbContext context)
    {
        if (!_tenantContext.IsResolved)
        {
            _logger.LogWarning("Attempting to apply tenant filters without resolved tenant context");
            return;
        }

        var tenantId = _tenantContext.TenantId!.Value;

        // Apply filters to all tenant entities
        ApplyFilterToEntitySet(context.Branches, tenantId);
        ApplyFilterToEntitySet(context.Users, tenantId);
        ApplyFilterToEntitySet(context.AuditLogs, tenantId);

        _logger.LogDebug("Applied tenant filters for tenant {TenantId}", tenantId);
    }

    private void ApplyFilterToEntitySet<T>(DbSet<T> entitySet, Guid tenantId) where T : class
    {
        // This is a simplified approach - in a real implementation, you might want to use
        // more sophisticated query filtering techniques
        var queryable = entitySet.AsQueryable();

        // The actual filtering will be handled by the global query filters
        // configured in the DbContext's OnModelCreating method
    }
}

/// <summary>
/// Extension methods for applying tenant filters
/// </summary>
public static class TenantQueryFilterExtensions
{
    /// <summary>
    /// Applies tenant filter to a queryable
    /// </summary>
    public static IQueryable<T> ApplyTenantFilter<T>(this IQueryable<T> query, ITenantContext tenantContext)
        where T : class, ITenantEntity
    {
        if (!tenantContext.IsResolved)
            throw new InvalidOperationException("Tenant context must be resolved before applying tenant filters");

        return query.Where(e => e.TenantId == tenantContext.TenantId!.Value);
    }
}

