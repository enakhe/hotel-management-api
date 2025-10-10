using HotelManagement.Application.Common.Interfaces.Tenant;

namespace HotelManagement.Application.Common.Services;

/// <summary>
/// Scoped service that provides tenant context for the current request
/// </summary>
public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public string? TenantIdentifier { get; private set; }
    public bool IsResolved => TenantId.HasValue && !string.IsNullOrEmpty(TenantIdentifier);

    public void SetTenant(Guid tenantId, string tenantIdentifier)
    {
        TenantId = tenantId;
        TenantIdentifier = tenantIdentifier;
    }
}

