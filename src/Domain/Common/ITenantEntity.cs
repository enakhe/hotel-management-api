namespace HotelManagement.Domain.Common;

/// <summary>
/// Interface for entities that belong to a specific tenant
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// The ID of the tenant that owns this entity
    /// </summary>
    Guid TenantId { get; set; }
}
