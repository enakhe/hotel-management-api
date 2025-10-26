using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities.SuperAdmin;

/// <summary>
/// Represents a module in the hotel management system
/// </summary>
public class Module
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Category { get; set; }

    /// <summary>
    /// Whether this module is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is a core module (required for basic functionality)
    /// </summary>
    public bool IsCore { get; set; } = false;

    /// <summary>
    /// List of module IDs that this module depends on
    /// </summary>
    public string? Dependencies { get; set; } // JSON array of module IDs

    /// <summary>
    /// Features included in this module
    /// </summary>
    [JsonIgnore]
    public ICollection<ModuleFeature> Features { get; set; } = new List<ModuleFeature>();

    /// <summary>
    /// Pricing information for this module
    /// </summary>
    [JsonIgnore]
    public ModulePricing? Pricing { get; set; }

    /// <summary>
    /// When this module was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this module was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who created this module
    /// </summary>
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Who last updated this module
    /// </summary>
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Represents a feature within a module
/// </summary>
public class ModuleFeature
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether this feature is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Configuration settings for this feature (JSON)
    /// </summary>
    public string? Configuration { get; set; }

    /// <summary>
    /// Foreign key to the Module this feature belongs to
    /// </summary>
    public Guid ModuleId { get; set; }

    /// <summary>
    /// Navigation property to the Module
    /// </summary>
    [JsonIgnore]
    public Module? Module { get; set; }
}

/// <summary>
/// Represents pricing information for a module
/// </summary>
public class ModulePricing
{
    public Guid Id { get; set; }

    /// <summary>
    /// Type of pricing model
    /// </summary>
    public PricingType Type { get; set; }

    /// <summary>
    /// Price amount (if applicable)
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Currency code (e.g., USD, EUR)
    /// </summary>
    [MaxLength(3)]
    public string? Currency { get; set; }

    /// <summary>
    /// Billing cycle for recurring pricing
    /// </summary>
    public BillingCycle? BillingCycle { get; set; }

    /// <summary>
    /// Minimum quantity for usage-based pricing
    /// </summary>
    public int? MinQuantity { get; set; }

    /// <summary>
    /// Maximum quantity for usage-based pricing
    /// </summary>
    public int? MaxQuantity { get; set; }

    /// <summary>
    /// Foreign key to the Module this pricing belongs to
    /// </summary>
    public Guid ModuleId { get; set; }

    /// <summary>
    /// Navigation property to the Module
    /// </summary>
    [JsonIgnore]
    public Module? Module { get; set; }
}
