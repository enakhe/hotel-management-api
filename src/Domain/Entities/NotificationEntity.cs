using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Persistent notification entity stored in database
/// </summary>
public class NotificationEntity : BaseTenantEntity
{
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// User ID this notification is for
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Action URL (e.g., link to invoice, booking)
    /// </summary>
    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Additional data (JSON)
    /// </summary>
    public string? Data { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public bool IsRead { get; set; }
    public bool IsDelivered { get; set; }
    public bool IsArchived { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual ApplicationUser User { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<NotificationDelivery> Deliveries { get; set; } = new List<NotificationDelivery>();
}

/// <summary>
/// Notification types
/// </summary>
public enum NotificationType
{
    Info = 0,
    Success = 1,
    Warning = 2,
    Error = 3
}

/// <summary>
/// Notification priority levels
/// </summary>
public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}

/// <summary>
/// Notification delivery channels
/// </summary>
public enum NotificationChannel
{
    InApp = 0,
    Email = 1,
    Sms = 2,
    Push = 3,
    All = 4
}

/// <summary>
/// Tracks delivery status for each channel
/// </summary>
public class NotificationDelivery : BaseEntity
{
    public Guid NotificationId { get; set; }
    public NotificationChannel Channel { get; set; }
    public DeliveryStatus Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? FailedAt { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; }
    public int MaxRetries { get; set; } = 3;

    [MaxLength(200)]
    public string? ExternalId { get; set; } // Gateway message ID

    // Navigation properties
    [JsonIgnore]
    public virtual NotificationEntity Notification { get; set; } = null!;
}

/// <summary>
/// Delivery status
/// </summary>
public enum DeliveryStatus
{
    Pending = 0,
    Sending = 1,
    Sent = 2,
    Delivered = 3,
    Failed = 4,
    Bounced = 5,
    Rejected = 6
}

/// <summary>
/// Notification templates
/// </summary>
public class NotificationTemplate : BaseTenantEntity
{
    [Required]
    [MaxLength(100)]
    public string TemplateKey { get; set; } = string.Empty; // e.g., "invoice_created"

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public NotificationCategory Category { get; set; }

    // Template content for different channels
    [MaxLength(200)]
    public string? EmailSubject { get; set; }

    public string? EmailBody { get; set; } // HTML

    [MaxLength(500)]
    public string? SmsBody { get; set; } // Plain text, 160 chars limit

    [MaxLength(500)]
    public string? PushTitle { get; set; }

    [MaxLength(500)]
    public string? PushBody { get; set; }

    public string? InAppBody { get; set; } // Can be HTML or markdown

    /// <summary>
    /// Template variables (JSON array of variable names)
    /// </summary>
    public string? Variables { get; set; } // e.g., ["TenantName", "Amount", "DueDate"]

    [MaxLength(10)]
    public string Language { get; set; } = "en";

    public bool IsActive { get; set; } = true;
    public bool IsSystem { get; set; } // System templates cannot be deleted

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [MaxLength(200)]
    public string? CreatedBy { get; set; }

    [MaxLength(200)]
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Notification categories
/// </summary>
public enum NotificationCategory
{
    System = 0,
    Billing = 1,
    Booking = 2,
    Account = 3,
    Alert = 4,
    Marketing = 5,
    Update = 6
}

/// <summary>
/// User notification preferences
/// </summary>
public class UserNotificationPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public NotificationCategory Category { get; set; }

    // Channel preferences
    public bool EmailEnabled { get; set; } = true;
    public bool SmsEnabled { get; set; } = true;
    public bool PushEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;

    // Quiet hours (time of day)
    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }

    [MaxLength(10)]
    public string? PreferredLanguage { get; set; }

    public bool DigestEnabled { get; set; } // Receive daily/weekly digests
    public DigestFrequency DigestFrequency { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual ApplicationUser User { get; set; } = null!;
}

/// <summary>
/// Digest frequency
/// </summary>
public enum DigestFrequency
{
    None = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3
}

