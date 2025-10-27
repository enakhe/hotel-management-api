using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HotelManagement.Domain.Common;
using HotelManagement.Domain.Enums;
using HotelManagement.Domain.Entities.SuperAdmin;

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
    public string? TimeZone { get; set; } = "WAT";

    [MaxLength(10)]
    public string? CurrencyCode { get; set; } = "NGN";

    [MaxLength(10)]
    public string? LanguageCode { get; set; } = "en";

    // License & Subscription Management
    public bool IsActive { get; set; } = true;

    // Plan relationship - Tenant references Plan for limits
    public Guid PlanId { get; set; }

    [JsonIgnore]
    public virtual Plan Plan { get; set; } = null!;

    public Guid LicenseId { get; set; }

    [JsonIgnore]
    public virtual License License { get; set; } = null!;

    // Tenant Metadata
    [MaxLength(100)]
    public string? Industry { get; set; }

    [MaxLength(50)]
    public string? Country { get; set; }

    [MaxLength(50)]
    public string? Region { get; set; }

    // Navigation properties
    public ICollection<Branch> Branches { get; set; } = [];
}

