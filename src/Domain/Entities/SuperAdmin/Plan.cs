using System.ComponentModel.DataAnnotations;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities.SuperAdmin;

/// <summary>
/// Represents a subscription plan for the hotel management system
/// </summary>
public class Plan
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Price of the plan
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Currency code (e.g., USD, EUR, GBP)
    /// </summary>
    [Required]
    [MaxLength(3)]
    public required string Currency { get; set; }

    /// <summary>
    /// Billing cycle for this plan
    /// </summary>
    public BillingCycle BillingCycle { get; set; }

    /// <summary>
    /// Whether this plan is currently active and available for subscription
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this plan is marked as popular/recommended
    /// </summary>
    public bool IsPopular { get; set; } = false;

    /// <summary>
    /// List of modules included in this plan (stored as JSON array)
    /// </summary>
    public string? Modules { get; set; }

    /// <summary>
    /// When this plan was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this plan was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID of the user who created this plan
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// ID of the user who last updated this plan
    /// </summary>
    public string? UpdatedBy { get; set; }

    // Navigation properties
    /// <summary>
    /// Features included in this plan
    /// </summary>
    public ICollection<PlanFeature> Features { get; set; } = new List<PlanFeature>();

    /// <summary>
    /// Resource limits for this plan
    /// </summary>
    public PlanLimits? Limits { get; set; }
}
