using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Represents a feature that can be included in a subscription plan
/// </summary>
public class PlanFeature
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether this feature is included in the plan
    /// </summary>
    public bool Included { get; set; }

    /// <summary>
    /// Optional limit for this feature (e.g., 1000 for max reservations)
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Unit of measurement for the limit (e.g., "reservations", "GB", "users")
    /// </summary>
    [MaxLength(50)]
    public string? Unit { get; set; }

    /// <summary>
    /// Foreign key to the Plan this feature belongs to
    /// </summary>
    public Guid PlanId { get; set; }

    /// <summary>
    /// Navigation property to the Plan
    /// </summary>
    [JsonIgnore]
    public Plan? Plan { get; set; }
}
