using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

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
    /// Base price for this plan (can be auto-calculated from modules or manually set)
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// How the price is determined
    /// </summary>
    public PlanPricingStrategy PricingStrategy { get; set; } = PlanPricingStrategy.ModuleSum;

    /// <summary>
    /// Setup/onboarding fee (one-time charge)
    /// </summary>
    public decimal? SetupFee { get; set; }

    /// <summary>
    /// Discount percentage (0-100) applied to plan price
    /// </summary>
    public decimal? DiscountPercentage { get; set; }

    /// <summary>
    /// Whether this plan is currently active and available for subscription
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this plan is marked as popular/recommended
    /// </summary>
    public bool IsPopular { get; set; } = false;

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

    /// <summary>
    /// Resource limits for this plan - Plan owns the limits
    /// </summary>
    public Guid LimitsId { get; set; }

    [JsonIgnore]
    public virtual Limits Limits { get; set; } = null!;

    // Navigation properties to entities that use this plan
    [JsonIgnore]
    public virtual ICollection<Tenant> Tenants { get; set; } = [];

    [JsonIgnore]
    public virtual ICollection<License> Licenses { get; set; } = [];

    // Many-to-many relationship with Modules through PlanModule junction table
    [JsonIgnore]
    public virtual ICollection<PlanModule> PlanModules { get; set; } = [];

    // Relationship with Subscriptions
    [JsonIgnore]
    public virtual ICollection<Subscription> Subscriptions { get; set; } = [];
}
