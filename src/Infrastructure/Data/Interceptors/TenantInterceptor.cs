using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Data.Interceptors;

/// <summary>
/// Interceptor that automatically stamps TenantId on tenant entities
/// </summary>
public class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<TenantInterceptor> _logger;

    public TenantInterceptor(ITenantContext tenantContext, ILogger<TenantInterceptor> logger)
    {
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        StampTenantId(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        StampTenantId(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void StampTenantId(DbContext? context)
    {
        if (context == null || !_tenantContext.IsResolved)
            return;

        var tenantId = _tenantContext.TenantId!.Value;

        foreach (var entry in context.ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.TenantId = tenantId;
                _logger.LogDebug("Stamped TenantId {TenantId} on {EntityType} {EntityId}",
                    tenantId, entry.Entity.GetType().Name, entry.Entity.GetType().GetProperty("Id")?.GetValue(entry.Entity));
            }
            else if (entry.State == EntityState.Modified)
            {
                // Prevent changing TenantId on existing entities
                var originalTenantId = entry.OriginalValues.GetValue<Guid>("TenantId");
                if (originalTenantId != tenantId)
                {
                    _logger.LogWarning("Attempted to change TenantId from {OriginalTenantId} to {NewTenantId} on {EntityType} {EntityId}",
                        originalTenantId, tenantId, entry.Entity.GetType().Name, entry.Entity.GetType().GetProperty("Id")?.GetValue(entry.Entity));

                    // Revert the change
                    entry.Property(e => e.TenantId).CurrentValue = originalTenantId;
                }
            }
        }
    }
}

