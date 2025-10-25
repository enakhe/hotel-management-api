using System.ComponentModel.DataAnnotations;
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities.Configuration;

/// <summary>
/// Represents a tenant (hotel) in the multi-tenant system - serves as the Tenant Registry
/// </summary>
public class Tenant : BaseTenantAuditableEntity
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Identifier { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [Phone]
    public string? ContactNumber { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    // Tenant Registry Configuration
    [MaxLength(50)]
    public string? TimeZone { get; set; } = "UTC";

    [MaxLength(10)]
    public string? CurrencyCode { get; set; } = "USD";

    [MaxLength(10)]
    public string? LanguageCode { get; set; } = "en";

    // License & Subscription Management
    public bool IsActive { get; set; } = true;
    public DateTime? SubscriptionStartDate { get; set; } 
    public DateTime? SubscriptionEndDate { get; set; }

    [MaxLength(50)]
    public string? SubscriptionPlan { get; set; } = "Basic";

    public LicenseStatus LicenseStatus { get; set; } = LicenseStatus.Trial;
    public DateTime? LicenseExpiryDate { get; set; }
    public DateTime? LastLicenseCheck { get; set; }

    // Resource Limits
    public int MaxUsers { get; set; } = 10;
    public int MaxBranches { get; set; } = 1;
    public int MaxRooms { get; set; } = 100;
    public int MaxReservations { get; set; } = 1000;

    // Database Configuration (for future DB-per-tenant migration)
    [MaxLength(200)]
    public string? DatabaseConnectionString { get; set; }

    [MaxLength(50)]
    public string? DatabaseProvider { get; set; } = "SqlServer"; // SqlServer, PostgreSQL, MySQL

    public bool UseSharedDatabase { get; set; } = true;

    // Feature Flags (stored as JSON for flexibility)
    public string? FeatureFlags { get; set; } // JSON string of enabled features

    // Tenant Metadata
    [MaxLength(100)]
    public string? Industry { get; set; } // Hotel, Resort, Motel, etc.

    [MaxLength(50)]
    public string? Country { get; set; }

    [MaxLength(50)]
    public string? Region { get; set; }

    // Billing Information
    [MaxLength(100)]
    public string? BillingContactName { get; set; }

    [EmailAddress]
    public string? BillingEmail { get; set; }

    [MaxLength(200)]
    public string? BillingAddress { get; set; }

    // Navigation properties
    public ICollection<Branch> Branches { get; set; } = [];
    public ICollection<TenantFeature> Features { get; set; } = [];
}

