using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Represents an email subscription to a scheduled report
/// </summary>
public class ReportSubscription : BaseEntity
{
    /// <summary>
    /// SuperAdmin who owns this subscription
    /// </summary>
    public Guid SuperAdminId { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public required string Email { get; set; }

    /// <summary>
    /// Associated report schedule
    /// </summary>
    public Guid ReportScheduleId { get; set; }

    public virtual ReportSchedule ReportSchedule { get; set; } = null!;

    /// <summary>
    /// Whether the subscription is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Unique token for unsubscribing
    /// </summary>
    [MaxLength(100)]
    public string UnsubscribeToken { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Last time report was sent to this subscription
    /// </summary>
    public DateTime? LastSentAt { get; set; }

    /// <summary>
    /// Number of reports sent to this subscription
    /// </summary>
    public int SentCount { get; set; } = 0;
}

