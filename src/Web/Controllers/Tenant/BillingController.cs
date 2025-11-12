using Asp.Versioning;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Common.Security;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Controllers.Tenant;

/// <summary>
/// Tenant billing controller for viewing invoices and subscription
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/billing")]
[Authorize(Roles = "Administrator")]
[Produces("application/json")]
public class BillingController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IInvoicingService _invoicingService;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<BillingController> _logger;

    public BillingController(
        ISubscriptionService subscriptionService,
        IInvoicingService invoicingService,
        ITenantContext tenantContext,
        ILogger<BillingController> logger)
    {
        _subscriptionService = subscriptionService;
        _invoicingService = invoicingService;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    /// <summary>
    /// Get current subscription for tenant
    /// </summary>
    [HttpGet("subscription")]
    [ProducesResponseType(typeof(SubscriptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscription()
    {
        if (!_tenantContext.TenantId.HasValue)
            return BadRequest("Tenant context not resolved");

        var result = await _subscriptionService.GetSubscriptionByTenantAsync(_tenantContext.TenantId.Value);
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, string.Join(", ", result.Errors));
    }

    /// <summary>
    /// Get invoices for current tenant
    /// </summary>
    [HttpGet("invoices")]
    [ProducesResponseType(typeof(PaginatedResult<InvoiceResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoices([FromQuery] GetInvoicesRequest request)
    {
        if (!_tenantContext.TenantId.HasValue)
            return BadRequest("Tenant context not resolved");

        var result = await _invoicingService.GetInvoicesByTenantAsync(_tenantContext.TenantId.Value, request);
        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, string.Join(", ", result.Errors));
    }

    /// <summary>
    /// Get specific invoice
    /// </summary>
    [HttpGet("invoices/{id}")]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoice(Guid id)
    {
        var result = await _invoicingService.GetInvoiceByIdAsync(id);
        
        // Ensure tenant can only access their own invoices
        if (result.Succeeded && result.Data != null && result.Data.TenantId != _tenantContext.TenantId)
            return Forbid();

        return result.Succeeded ? Ok(result.Data) : StatusCode(result.StatusCode, string.Join(", ", result.Errors));
    }

    /// <summary>
    /// Download invoice PDF
    /// </summary>
    [HttpGet("invoices/{id}/download")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadInvoice(Guid id)
    {
        var invoice = await _invoicingService.GetInvoiceByIdAsync(id);
        
        if (!invoice.Succeeded || invoice.Data == null)
            return StatusCode(invoice.StatusCode, string.Join(", ", invoice.Errors));

        // Ensure tenant can only download their own invoices
        if (invoice.Data.TenantId != _tenantContext.TenantId)
            return Forbid();

        var pdfResult = await _invoicingService.GenerateInvoicePdfAsync(id);
        
        if (!pdfResult.Succeeded || pdfResult.Data == null)
            return StatusCode(pdfResult.StatusCode, string.Join(", ", pdfResult.Errors));

        return File(pdfResult.Data, "application/pdf", $"invoice-{invoice.Data.InvoiceNumber}.pdf");
    }

    /// <summary>
    /// Get payment history
    /// </summary>
    [HttpGet("payments")]
    [ProducesResponseType(typeof(List<PaymentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentHistory()
    {
        if (!_tenantContext.TenantId.HasValue)
            return BadRequest("Tenant context not resolved");

        // Get all invoices with payments
        var invoicesResult = await _invoicingService.GetInvoicesByTenantAsync(
            _tenantContext.TenantId.Value,
            new GetInvoicesRequest { PageSize = 100 });

        if (!invoicesResult.Succeeded || invoicesResult.Data == null)
            return StatusCode(invoicesResult.StatusCode, string.Join(", ", invoicesResult.Errors));

        var allPayments = invoicesResult.Data.Items
            .SelectMany(i => i.Payments)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

        return Ok(allPayments);
    }
}

