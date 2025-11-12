using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Tracks payment retry attempts for dunning management
/// </summary>
public class PaymentRetry : BaseTenantEntity
{
    public Guid InvoiceId { get; set; }

    public int AttemptNumber { get; set; }
    public DateTime AttemptedAt { get; set; }
    public DateTime? NextRetryAt { get; set; }

    public PaymentRetryStatus Status { get; set; }

    [MaxLength(1000)]
    public string? FailureReason { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Payment method attempted
    /// </summary>
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// Whether notification was sent to tenant
    /// </summary>
    public bool NotificationSent { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual Invoice Invoice { get; set; } = null!;

    [JsonIgnore]
    public virtual Tenant Tenant { get; set; } = null!;
}

/// <summary>
/// Payment retry status
/// </summary>
public enum PaymentRetryStatus
{
    Pending = 0,
    Attempted = 1,
    Success = 2,
    Failed = 3,
    Abandoned = 4
}

