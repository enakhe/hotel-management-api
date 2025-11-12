using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Defines overage pricing for usage-based billing
/// </summary>
public class OveragePricing : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public UsageMetric Metric { get; set; }

    /// <summary>
    /// Price per unit over the included amount
    /// </summary>
    public decimal PricePerUnit { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "NGN";

    /// <summary>
    /// Optional plan-specific pricing (if null, applies to all plans)
    /// </summary>
    public Guid? PlanId { get; set; }

    /// <summary>
    /// Minimum billable quantity (e.g., charge per 100 emails)
    /// </summary>
    public int? MinimumBillableUnit { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [MaxLength(200)]
    public string? CreatedBy { get; set; }

    [MaxLength(200)]
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual Plan? Plan { get; set; }
}

