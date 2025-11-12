using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Invoice response DTO
/// </summary>
public record InvoiceResponseDto
{
    public Guid Id { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public Guid SubscriptionId { get; init; }
    public string SubscriptionNumber { get; init; } = string.Empty;
    public DateTime IssueDate { get; init; }
    public DateTime DueDate { get; init; }
    public DateTime? PaidDate { get; init; }
    public InvoiceStatus Status { get; init; }
    public string StatusDisplay { get; init; } = string.Empty;
    public InvoiceType Type { get; init; }
    public string TypeDisplay { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal TaxRate { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal AmountDue { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public int PaymentTermsDays { get; init; }
    public string? Notes { get; init; }
    public string? PaymentInstructions { get; init; }
    public DateTime? SentAt { get; init; }
    public DateTime? ViewedAt { get; init; }
    public bool IsOverdue { get; init; }
    public int DaysOverdue { get; init; }
    public List<InvoiceLineItemDto> LineItems { get; init; } = new();
    public List<PaymentResponseDto> Payments { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Invoice line item DTO
/// </summary>
public record InvoiceLineItemDto
{
    public Guid Id { get; init; }
    public int LineNumber { get; init; }
    public string Description { get; init; } = string.Empty;
    public LineItemType Type { get; init; }
    public string TypeDisplay { get; init; } = string.Empty;
    public Guid? ModuleId { get; init; }
    public string? ModuleName { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Amount { get; init; }
    public bool IsTaxable { get; init; }
    public decimal? TaxRate { get; init; }
    public decimal? TaxAmount { get; init; }
    public DateTime? ServicePeriodStart { get; init; }
    public DateTime? ServicePeriodEnd { get; init; }
}

/// <summary>
/// Request to get invoices list
/// </summary>
public record GetInvoicesRequest
{
    public Guid? TenantId { get; init; }
    public Guid? SubscriptionId { get; init; }
    public InvoiceStatus? Status { get; init; }
    public InvoiceType? Type { get; init; }
    public DateTime? IssueDateFrom { get; init; }
    public DateTime? IssueDateTo { get; init; }
    public DateTime? DueDateFrom { get; init; }
    public DateTime? DueDateTo { get; init; }
    public bool? IsOverdue { get; init; }
    public string? SearchTerm { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "IssueDate";
    public bool SortDescending { get; init; } = true;
}


