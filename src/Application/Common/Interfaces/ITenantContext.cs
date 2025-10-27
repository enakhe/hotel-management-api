using HotelManagement.Domain.Common;

namespace HotelManagement.Application.Common.Interfaces.Tenant;

/// <summary>
/// Provides access to the current tenant context for the request
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// The current tenant ID for this request
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// The tenant identifier (slug/name) for this request
    /// </summary>
    string? TenantIdentifier { get; }

    /// <summary>
    /// Whether a valid tenant context has been resolved
    /// </summary>
    bool IsResolved { get; }

    /// <summary>
    /// Sets the tenant context for the current request
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="tenantIdentifier">The tenant identifier</param>
    void SetTenant(Guid tenantId, string tenantIdentifier);
}

