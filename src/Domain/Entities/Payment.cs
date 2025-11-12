using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Represents a payment made by a tenant
/// </summary>
public class Payment : BaseTenantEntity
{
    [Required]
    [MaxLength(50)]
    public string PaymentReference { get; set; } = string.Empty; // PAY-2024-0001

    public Guid InvoiceId { get; set; }
    public Guid? SubscriptionId { get; set; }

    public decimal Amount { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "NGN";

    public PaymentMethod Method { get; set; }
    public Enums.PaymentStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? SettledAt { get; set; }

    // Gateway details
    [MaxLength(200)]
    public string? GatewayTransactionId { get; set; }

    [MaxLength(200)]
    public string? GatewayReference { get; set; }

    public PaymentGateway Gateway { get; set; }

    /// <summary>
    /// Full gateway response (JSON)
    /// </summary>
    public string? GatewayResponse { get; set; }

    // Payment method details (masked for security)
    [MaxLength(500)]
    public string? PaymentMethodDetails { get; set; }

    [MaxLength(4)]
    public string? Last4Digits { get; set; }

    [MaxLength(50)]
    public string? CardBrand { get; set; }

    [MaxLength(1000)]
    public string? FailureReason { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    // Reconciliation
    public bool IsReconciled { get; set; }
    public DateTime? ReconciledAt { get; set; }

    [MaxLength(200)]
    public string? ReconciledBy { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual Invoice Invoice { get; set; } = null!;

    [JsonIgnore]
    public virtual Tenant Tenant { get; set; } = null!;

    [JsonIgnore]
    public virtual Subscription? Subscription { get; set; }
}


