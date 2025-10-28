using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Represents features/modules available to a tenant
/// </summary>
public class TenantFeature : BaseTenantEntity
{
    [Required]
    [MaxLength(50)]
    public required string FeatureName { get; set; }

    public bool IsEnabled { get; set; } = false;

    public DateTime? EnabledAt { get; set; }
    public DateTime? DisabledAt { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }

    // Navigation property
    public required Tenant Tenant { get; set; }
}
