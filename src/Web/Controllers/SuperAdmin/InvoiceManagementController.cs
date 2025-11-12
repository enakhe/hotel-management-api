using Asp.Versioning;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Common.Security;
using HotelManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.SuperAdmin;

/// <summary>
/// SuperAdmin invoice management controller
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("cp/api/v{version:apiVersion}/invoices")]
[Authorize(Roles = "SuperAdministrator")]
[Produces("application/json")]
public class InvoiceManagementController : ControllerBase
{
    private readonly IInvoicingService _invoicingService;
    private readonly ILogger<InvoiceManagementController> _logger;

    public InvoiceManagementController(
        IInvoicingService invoicingService,
        ILogger<InvoiceManagementController> logger)
    {
        _invoicingService = invoicingService;
        _logger = logger;
    }

    /// <summary>
    /// Get all invoices
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<InvoiceResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoices([FromQuery] GetInvoicesRequest request)
    {
        var result = await _invoicingService.GetInvoicesAsync(request);
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Get invoice by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoice(Guid id)
    {
        var result = await _invoicingService.GetInvoiceByIdAsync(id);
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Get invoice by invoice number
    /// </summary>
    [HttpGet("number/{invoiceNumber}")]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoiceByNumber(string invoiceNumber)
    {
        var result = await _invoicingService.GetInvoiceByNumberAsync(invoiceNumber);
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Get invoices for a specific tenant
    /// </summary>
    [HttpGet("tenant/{tenantId}")]
    [ProducesResponseType(typeof(PaginatedResult<InvoiceResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoicesByTenant(Guid tenantId, [FromQuery] GetInvoicesRequest request)
    {
        var result = await _invoicingService.GetInvoicesByTenantAsync(tenantId, request);
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Generate invoice for a subscription
    /// </summary>
    [HttpPost("generate/{subscriptionId}")]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateInvoice(Guid subscriptionId, [FromQuery] InvoiceType type = InvoiceType.Recurring)
    {
        var result = await _invoicingService.GenerateInvoiceAsync(subscriptionId, type);
        
        if (result.Succeeded)
        {
            return CreatedAtAction(
                nameof(GetInvoice),
                new { id = result.Data!.Id },
                result.Data);
        }

        return StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Send invoice via email
    /// </summary>
    [HttpPost("{id}/send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendInvoice(Guid id)
    {
        var result = await _invoicingService.SendInvoiceEmailAsync(id);
        return result.Succeeded 
            ? Ok(new { success = true, message = "Invoice sent successfully" }) 
            : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Mark invoice as paid
    /// </summary>
    [HttpPost("{id}/mark-paid")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkInvoiceAsPaid(Guid id, [FromBody] MarkInvoiceAsPaidRequest request)
    {
        // First record the payment
        var paymentRequest = new RecordManualPaymentRequest
        {
            InvoiceId = id,
            Amount = request.Amount,
            Method = request.Method,
            PaymentDate = request.PaymentDate,
            TransactionReference = request.TransactionReference,
            Notes = request.Notes
        };

        var paymentResult = await _invoicingService.RecordManualPaymentAsync(paymentRequest);
        
        if (!paymentResult.Succeeded)
            return StatusCode(paymentResult.StatusCode, paymentResult.Errors);

        return Ok(new { success = true, payment = paymentResult.Data });
    }

    /// <summary>
    /// Record manual payment for an invoice
    /// </summary>
    [HttpPost("{id}/record-payment")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordManualPayment(Guid id, [FromBody] RecordManualPaymentRequest request)
    {
        // Ensure invoice ID matches
        if (request.InvoiceId != id)
            return BadRequest("Invoice ID mismatch");

        var result = await _invoicingService.RecordManualPaymentAsync(request);
        return result.Succeeded 
            ? CreatedAtAction(nameof(GetInvoice), new { id }, result.Data)
            : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Get overdue invoices
    /// </summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(List<InvoiceResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdueInvoices()
    {
        var result = await _invoicingService.GetOverdueInvoicesAsync();
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Get upcoming invoices (to be generated soon)
    /// </summary>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(List<SubscriptionResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcomingInvoices([FromQuery] int daysAhead = 7)
    {
        var result = await _invoicingService.GetUpcomingInvoicesAsync(daysAhead);
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Cancel/void an invoice
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelInvoice(Guid id, [FromBody] CancelInvoiceRequest request)
    {
        var result = await _invoicingService.CancelInvoiceAsync(id, request.Reason);
        return result.Succeeded 
            ? Ok(new { success = true }) 
            : StatusCode(result.StatusCode, result.Errors);
    }

    /// <summary>
    /// Generate invoice PDF
    /// </summary>
    [HttpGet("{id}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> GenerateInvoicePdf(Guid id)
    {
        var result = await _invoicingService.GenerateInvoicePdfAsync(id);
        
        if (!result.Succeeded)
            return StatusCode(result.StatusCode, result.Errors);

        return File(result.Data!, "application/pdf", $"invoice-{id}.pdf");
    }
}

/// <summary>
/// Request to mark invoice as paid
/// </summary>
public record MarkInvoiceAsPaidRequest
{
    public decimal Amount { get; init; }
    public PaymentMethod Method { get; init; }
    public DateTime PaymentDate { get; init; } = DateTime.UtcNow;
    public string? TransactionReference { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Request to cancel invoice
/// </summary>
public record CancelInvoiceRequest
{
    public string Reason { get; init; } = string.Empty;
}

