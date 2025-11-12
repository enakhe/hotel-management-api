using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Tracks the onboarding progress of a new tenant
/// </summary>
public class TenantOnboarding : BaseEntity
{
    public Guid TenantId { get; set; }

    public OnboardingStatus Status { get; set; }
    public OnboardingStep CurrentStep { get; set; }

    [MaxLength(200)]
    public string? ContactPerson { get; set; }

    [MaxLength(200)]
    public string? ContactEmail { get; set; }

    [MaxLength(50)]
    public string? ContactPhone { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Number of rooms the hotel has
    /// </summary>
    public int? NumberOfRooms { get; set; }

    /// <summary>
    /// Preferred plan selected during onboarding
    /// </summary>
    public Guid? SelectedPlanId { get; set; }

    /// <summary>
    /// Customization choices (JSON)
    /// </summary>
    public string? CustomizationData { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual Tenant? Tenant { get; set; }

    [JsonIgnore]
    public virtual Plan? SelectedPlan { get; set; }
}


