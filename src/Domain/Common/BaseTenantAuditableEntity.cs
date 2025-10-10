namespace HotelManagement.Domain.Common;

/// <summary>
/// Base auditable entity for tenant-owned entities with audit fields
/// </summary>
public abstract class BaseTenantAuditableEntity : BaseTenantEntity
{
    public DateTimeOffset Created { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
}
