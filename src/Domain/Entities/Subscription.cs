using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Represents a tenant's subscription to a plan
/// </summary>
public class Subscription : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string SubscriptionNumber { get; set; } = string.Empty; // SUB-2024-0001

    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? TrialEndDate { get; set; }

    public SubscriptionStatus Status { get; set; }
    public BillingCycle BillingCycle { get; set; }

    /// <summary>
    /// Locked-in monthly price when subscription was created
    /// </summary>
    public decimal MonthlyPrice { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "NGN";

    /// <summary>
    /// Custom discount percentage (0-100)
    /// </summary>
    public decimal? CustomDiscount { get; set; }

    [MaxLength(500)]
    public string? DiscountReason { get; set; }

    public DateTime NextBillingDate { get; set; }
    public DateTime? LastBillingDate { get; set; }

    public bool AutoRenew { get; set; } = true;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual Tenant Tenant { get; set; } = null!;

    [JsonIgnore]
    public virtual Plan Plan { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [JsonIgnore]
    public virtual ICollection<SubscriptionModule> SubscriptionModules { get; set; } = new List<SubscriptionModule>();
}

/// <summary>
/// Junction table tracking which modules are included in a subscription
/// </summary>
public class SubscriptionModule : BaseEntity
{
    public Guid SubscriptionId { get; set; }
    public Guid ModuleId { get; set; }

    /// <summary>
    /// Price of module when subscription was created (locked-in price)
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Whether this module is optional (can be removed)
    /// </summary>
    public bool IsOptional { get; set; }

    public DateTime AddedAt { get; set; }
    public DateTime? RemovedAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual Subscription Subscription { get; set; } = null!;

    [JsonIgnore]
    public virtual Module Module { get; set; } = null!;
}


