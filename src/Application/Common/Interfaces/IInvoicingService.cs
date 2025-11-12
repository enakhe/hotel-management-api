using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for managing invoices
/// </summary>
public interface IInvoicingService
{
    /// <summary>
    /// Generate an invoice for a subscription
    /// </summary>
    Task<Result<InvoiceResponseDto>> GenerateInvoiceAsync(
        Guid subscriptionId,
        InvoiceType type,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get invoice by ID
    /// </summary>
    Task<Result<InvoiceResponseDto>> GetInvoiceByIdAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get invoice by invoice number
    /// </summary>
    Task<Result<InvoiceResponseDto>> GetInvoiceByNumberAsync(
        string invoiceNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get invoices for a specific tenant
    /// </summary>
    Task<Result<PaginatedResult<InvoiceResponseDto>>> GetInvoicesByTenantAsync(
        Guid tenantId,
        GetInvoicesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all invoices (SuperAdmin only)
    /// </summary>
    Task<Result<PaginatedResult<InvoiceResponseDto>>> GetInvoicesAsync(
        GetInvoicesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send invoice via email to tenant
    /// </summary>
    Task<Result<bool>> SendInvoiceEmailAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark invoice as paid
    /// </summary>
    Task<Result<bool>> MarkInvoiceAsPaidAsync(
        Guid invoiceId,
        Guid paymentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Record manual payment for an invoice
    /// </summary>
    Task<Result<PaymentResponseDto>> RecordManualPaymentAsync(
        RecordManualPaymentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overdue invoices
    /// </summary>
    Task<Result<List<InvoiceResponseDto>>> GetOverdueInvoicesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get upcoming invoices (to be generated soon)
    /// </summary>
    Task<Result<List<SubscriptionResponseDto>>> GetUpcomingInvoicesAsync(
        int daysAhead = 7,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel/void an invoice
    /// </summary>
    Task<Result<bool>> CancelInvoiceAsync(
        Guid invoiceId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate invoice PDF
    /// </summary>
    Task<Result<byte[]>> GenerateInvoicePdfAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default);
}


