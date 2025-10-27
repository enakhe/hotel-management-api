using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HotelManagement.Domain.Entities.SuperAdmin;

/// <summary>
/// Junction table for many-to-many relationship between Plan and Module
/// Represents which modules are included in a specific plan
/// </summary>
public class PlanModule
{
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the Plan
    /// </summary>
    public Guid PlanId { get; set; }

    /// <summary>
    /// Foreign key to the Module
    /// </summary>
    public Guid ModuleId { get; set; }

    /// <summary>
    /// Whether this module is required in this plan (cannot be removed)
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// Order/priority of this module in the plan (for display purposes)
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// When this module was added to the plan
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who added this module to the plan
    /// </summary>
    [MaxLength(100)]
    public string? AddedBy { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual Plan Plan { get; set; } = null!;

    [JsonIgnore]
    public virtual Module Module { get; set; } = null!;
}
