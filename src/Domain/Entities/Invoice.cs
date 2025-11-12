using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Represents an invoice sent to a tenant
/// </summary>
public class Invoice : BaseTenantEntity
{
    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty; // INV-2024-0001

    public Guid SubscriptionId { get; set; }

    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }

    public InvoiceStatus Status { get; set; }
    public InvoiceType Type { get; set; }

    // Amounts
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; } // e.g., 7.5 for 7.5% VAT
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "NGN";

    // Billing period this invoice covers
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Payment terms in days (e.g., 7 means due in 7 days)
    /// </summary>
    public int PaymentTermsDays { get; set; } = 7;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(1000)]
    public string? PaymentInstructions { get; set; }

    /// <summary>
    /// Date when invoice was sent to customer
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Date when customer viewed invoice
    /// </summary>
    public DateTime? ViewedAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual Tenant Tenant { get; set; } = null!;

    [JsonIgnore]
    public virtual Subscription Subscription { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();

    [JsonIgnore]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

/// <summary>
/// Represents a line item in an invoice
/// </summary>
public class InvoiceLineItem : BaseEntity
{
    public Guid InvoiceId { get; set; }

    public int LineNumber { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public LineItemType Type { get; set; }

    /// <summary>
    /// Reference to module if this line item is for a module
    /// </summary>
    public Guid? ModuleId { get; set; }

    /// <summary>
    /// Reference to plan if this line item is for plan subscription
    /// </summary>
    public Guid? PlanId { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }

    public bool IsTaxable { get; set; } = true;
    public decimal? TaxRate { get; set; }
    public decimal? TaxAmount { get; set; }

    /// <summary>
    /// Service period for this line item
    /// </summary>
    public DateTime? ServicePeriodStart { get; set; }
    public DateTime? ServicePeriodEnd { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual Invoice Invoice { get; set; } = null!;

    [JsonIgnore]
    public virtual Module? Module { get; set; }

    [JsonIgnore]
    public virtual Plan? Plan { get; set; }
}


