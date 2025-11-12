using System.Text.Json.Serialization;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Centralized limits configuration entity
/// This is the single source of truth for all limit configurations
/// </summary>
public class Limits : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Core Limits
    public int MaxUsers { get; set; } = -1; // -1 = unlimited
    public int MaxBranches { get; set; } = -1;
    public int MaxRooms { get; set; } = -1;
    public int MaxReservations { get; set; } = -1;
    public int MaxStorageGB { get; set; } = -1;
    public int ApiRateLimit { get; set; } = -1; // requests per hour
    public int ConcurrentSessions { get; set; } = -1;

    // Additional Limits
    public int MaxGuests { get; set; } = -1;
    public int MaxBookings { get; set; } = -1;
    public int MaxReports { get; set; } = -1;
    public int MaxIntegrations { get; set; } = -1;

    // Communication Limits
    /// <summary>
    /// Maximum number of emails that can be sent per month
    /// </summary>
    public int MaxEmailsPerMonth { get; set; } = -1; // -1 = unlimited

    /// <summary>
    /// Maximum number of SMS messages that can be sent per month
    /// </summary>
    public int MaxSmsPerMonth { get; set; } = -1; // -1 = unlimited

    // Custom Limits (JSON)
    public string? CustomLimits { get; set; } // JSON object for extensibility

    // Metadata
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = string.Empty;
    public string? LastModifiedBy { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual ICollection<Plan> Plans { get; set; } = new List<Plan>();

    [JsonIgnore]
    public virtual ICollection<Tenant> TenantOverrides { get; set; } = new List<Tenant>();
}

/// <summary>
/// Limits template for creating new limit configurations
/// </summary>
public class LimitsTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // e.g., "Basic", "Premium", "Enterprise"
    public string TemplateData { get; set; } = string.Empty; // JSON template
    public bool IsActive { get; set; } = true;
}
