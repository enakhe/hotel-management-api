using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Tracks usage metrics for billing purposes
/// </summary>
public class UsageRecord : BaseTenantEntity
{
    public Guid? SubscriptionId { get; set; }

    public UsageMetric Metric { get; set; }
    public decimal Quantity { get; set; }
    public decimal? Cost { get; set; }

    public DateTime RecordedAt { get; set; }
    public DateTime BillingPeriodStart { get; set; }
    public DateTime BillingPeriodEnd { get; set; }

    public bool IsBilled { get; set; }
    public Guid? InvoiceId { get; set; }

    [MaxLength(500)]
    public string? Metadata { get; set; } // JSON for additional data

    // Navigation properties
    [JsonIgnore]
    public virtual Tenant Tenant { get; set; } = null!;

    [JsonIgnore]
    public virtual Subscription? Subscription { get; set; }

    [JsonIgnore]
    public virtual Invoice? Invoice { get; set; }
}

/// <summary>
/// Types of usage metrics tracked
/// </summary>
public enum UsageMetric
{
    /// <summary>
    /// Number of emails sent
    /// </summary>
    EmailsSent = 0,

    /// <summary>
    /// Number of SMS messages sent
    /// </summary>
    SmsSent = 1,

    /// <summary>
    /// Number of push notifications sent
    /// </summary>
    PushNotificationsSent = 2,

    /// <summary>
    /// Storage used in GB
    /// </summary>
    StorageGB = 3,

    /// <summary>
    /// Number of API calls made
    /// </summary>
    ApiCalls = 4,

    /// <summary>
    /// Number of users created
    /// </summary>
    Users = 5,

    /// <summary>
    /// Number of bookings/reservations
    /// </summary>
    Bookings = 6,

    /// <summary>
    /// Number of transactions processed
    /// </summary>
    Transactions = 7
}

/// <summary>
/// Aggregated usage summary for a billing period
/// </summary>
public class UsageAggregation : BaseTenantEntity
{
    public Guid SubscriptionId { get; set; }

    public DateTime BillingPeriodStart { get; set; }
    public DateTime BillingPeriodEnd { get; set; }

    // Email usage
    public int EmailsSent { get; set; }
    public int EmailsIncluded { get; set; }
    public int EmailsOverage { get; set; }
    public decimal EmailOverageCost { get; set; }

    // SMS usage
    public int SmsSent { get; set; }
    public int SmsIncluded { get; set; }
    public int SmsOverage { get; set; }
    public decimal SmsOverageCost { get; set; }

    // Storage usage
    public decimal StorageUsedGB { get; set; }
    public decimal StorageIncludedGB { get; set; }
    public decimal StorageOverageGB { get; set; }
    public decimal StorageOverageCost { get; set; }

    // API usage
    public int ApiCallsMade { get; set; }
    public int ApiCallsIncluded { get; set; }
    public int ApiCallsOverage { get; set; }
    public decimal ApiCallsOverageCost { get; set; }

    public decimal TotalOverageCost { get; set; }

    public bool IsFinalized { get; set; }
    public DateTime? FinalizedAt { get; set; }

    public bool IsBilled { get; set; }
    public Guid? InvoiceId { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual Tenant Tenant { get; set; } = null!;

    [JsonIgnore]
    public virtual Subscription Subscription { get; set; } = null!;

    [JsonIgnore]
    public virtual Invoice? Invoice { get; set; }
}

