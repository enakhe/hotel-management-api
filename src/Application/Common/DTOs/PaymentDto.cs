using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Payment response DTO
/// </summary>
public record PaymentResponseDto
{
    public Guid Id { get; init; }
    public string PaymentReference { get; init; } = string.Empty;
    public Guid InvoiceId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public PaymentMethod Method { get; init; }
    public string MethodDisplay { get; init; } = string.Empty;
    public PaymentStatus Status { get; init; }
    public string StatusDisplay { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public DateTime? SettledAt { get; init; }
    public string? GatewayTransactionId { get; init; }
    public PaymentGateway Gateway { get; init; }
    public string GatewayDisplay { get; init; } = string.Empty;
    public string? Last4Digits { get; init; }
    public string? CardBrand { get; init; }
    public string? FailureReason { get; init; }
    public bool IsReconciled { get; init; }
    public DateTime? ReconciledAt { get; init; }
}

/// <summary>
/// Request to record a manual payment
/// </summary>
public record RecordManualPaymentRequest
{
    public Guid InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public PaymentMethod Method { get; init; }
    public DateTime PaymentDate { get; init; } = DateTime.UtcNow;
    public string? TransactionReference { get; init; }
    public string? Notes { get; init; }
}


