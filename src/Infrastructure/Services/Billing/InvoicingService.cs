using AutoMapper;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentStatus = HotelManagement.Domain.Enums.PaymentStatus;

namespace HotelManagement.Infrastructure.Services.Billing;

public class InvoicingService : IInvoicingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InvoicingService> _logger;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IUsageTrackingService _usageTrackingService;
    private const decimal DEFAULT_TAX_RATE = 7.5m; // 7.5% VAT

    public InvoicingService(
        ApplicationDbContext context,
        ILogger<InvoicingService> logger,
        IMapper mapper,
        IEmailService emailService,
        IUsageTrackingService usageTrackingService)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
        _emailService = emailService;
        _usageTrackingService = usageTrackingService;
    }

    public async Task<Result<InvoiceResponseDto>> GenerateInvoiceAsync(
        Guid subscriptionId,
        InvoiceType type,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Tenant)
                .Include(s => s.Plan)
                    .ThenInclude(p => p.PlanModules)
                        .ThenInclude(pm => pm.Module)
                .Include(s => s.SubscriptionModules)
                    .ThenInclude(sm => sm.Module)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);

            if (subscription == null)
                return Result<InvoiceResponseDto>.Failure("Subscription not found", 404);

            // Generate invoice number
            var invoiceCount = await _context.Invoices.CountAsync(cancellationToken);
            var invoiceNumber = $"INV-{DateTime.UtcNow.Year}-{(invoiceCount + 1):D4}";

            // Determine billing period
            var periodStart = type == InvoiceType.Initial
                ? subscription.StartDate
                : subscription.LastBillingDate ?? subscription.StartDate;
            var periodEnd = subscription.NextBillingDate;

            // Create invoice
            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = invoiceNumber,
                TenantId = subscription.TenantId,
                SubscriptionId = subscription.Id,
                IssueDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7), // 7 days payment terms
                Status = InvoiceStatus.Draft,
                Type = type,
                Currency = subscription.Currency,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                PaymentTermsDays = 7,
                TaxRate = DEFAULT_TAX_RATE
            };

            _context.Invoices.Add(invoice);

            var lineItems = new List<InvoiceLineItem>();
            int lineNumber = 1;

            // Add setup fee for initial invoices
            if (type == InvoiceType.Initial && subscription.Plan.SetupFee.HasValue && subscription.Plan.SetupFee.Value > 0)
            {
                lineItems.Add(new InvoiceLineItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    LineNumber = lineNumber++,
                    Description = "One-time Setup Fee",
                    Type = LineItemType.SetupFee,
                    Quantity = 1,
                    UnitPrice = subscription.Plan.SetupFee.Value,
                    Amount = subscription.Plan.SetupFee.Value,
                    IsTaxable = true
                });
            }

            // Add subscription line item
            lineItems.Add(new InvoiceLineItem
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                LineNumber = lineNumber++,
                Description = $"{subscription.Plan.Name} - Monthly Subscription",
                Type = LineItemType.Subscription,
                PlanId = subscription.PlanId,
                Quantity = 1,
                UnitPrice = subscription.MonthlyPrice,
                Amount = subscription.MonthlyPrice,
                IsTaxable = true,
                ServicePeriodStart = periodStart,
                ServicePeriodEnd = periodEnd
            });

            // Add individual module line items for transparency
            foreach (var subModule in subscription.SubscriptionModules.Where(sm => !sm.RemovedAt.HasValue))
            {
                lineItems.Add(new InvoiceLineItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    LineNumber = lineNumber++,
                    Description = $"  • {subModule.Module?.Name ?? "Module"}",
                    Type = LineItemType.Module,
                    ModuleId = subModule.ModuleId,
                    Quantity = 1,
                    UnitPrice = subModule.Price,
                    Amount = 0, // Already included in subscription price
                    IsTaxable = false // Don't double-tax
                });
            }

            // Add usage overage charges (for recurring invoices)
            if (type == InvoiceType.Recurring || type == InvoiceType.Overage)
            {
                var overageResult = await _usageTrackingService.CalculateOveragesAsync(
                    subscriptionId, periodStart, periodEnd, cancellationToken);

                if (overageResult.Succeeded && overageResult.Data != null && overageResult.Data.HasOverages)
                {
                    foreach (var overageItem in overageResult.Data.OverageItems)
                    {
                        var overageType = overageItem.Metric switch
                        {
                            UsageMetric.EmailsSent => LineItemType.EmailOverage,
                            UsageMetric.SmsSent => LineItemType.SmsOverage,
                            UsageMetric.StorageGB => LineItemType.StorageOverage,
                            _ => LineItemType.Module
                        };

                        lineItems.Add(new InvoiceLineItem
                        {
                            Id = Guid.NewGuid(),
                            InvoiceId = invoice.Id,
                            LineNumber = lineNumber++,
                            Description = $"{overageItem.MetricDisplay} Overage ({overageItem.QuantityOverage:N0} units over limit)",
                            Type = overageType,
                            Quantity = overageItem.QuantityOverage,
                            UnitPrice = overageItem.PricePerUnit,
                            Amount = overageItem.TotalCost,
                            IsTaxable = true
                        });
                    }
                }
            }

            // Calculate totals
            var taxableAmount = lineItems.Where(li => li.IsTaxable).Sum(li => li.Amount);
            invoice.Subtotal = taxableAmount;
            invoice.TaxAmount = taxableAmount * (DEFAULT_TAX_RATE / 100);
            invoice.DiscountAmount = 0;
            invoice.TotalAmount = invoice.Subtotal + invoice.TaxAmount;
            invoice.AmountDue = invoice.TotalAmount;
            invoice.AmountPaid = 0;

            // Add line items to context
            foreach (var lineItem in lineItems)
            {
                _context.InvoiceLineItems.Add(lineItem);
            }

            // Update invoice status
            invoice.Status = InvoiceStatus.Pending;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Generated invoice {InvoiceNumber} for subscription {SubscriptionNumber}",
                invoiceNumber, subscription.SubscriptionNumber);

            return await GetInvoiceByIdAsync(invoice.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice for subscription {SubscriptionId}", subscriptionId);
            return Result<InvoiceResponseDto>.Failure("An error occurred while generating invoice", 500);
        }
    }

    public async Task<Result<InvoiceResponseDto>> GetInvoiceByIdAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _context.Invoices
                .Include(i => i.Tenant)
                .Include(i => i.Subscription)
                .Include(i => i.LineItems.OrderBy(li => li.LineNumber))
                    .ThenInclude(li => li.Module)
                .Include(i => i.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

            if (invoice == null)
                return Result<InvoiceResponseDto>.Failure("Invoice not found", 404);

            var dto = _mapper.Map<InvoiceResponseDto>(invoice);

            // Calculate if overdue
            if (invoice.Status != InvoiceStatus.Paid && invoice.DueDate < DateTime.UtcNow)
            {
                dto = dto with
                {
                    IsOverdue = true,
                    DaysOverdue = (DateTime.UtcNow - invoice.DueDate).Days
                };
            }

            return Result<InvoiceResponseDto>.Success(dto, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoice {InvoiceId}", invoiceId);
            return Result<InvoiceResponseDto>.Failure("An error occurred while getting invoice", 500);
        }
    }

    public async Task<Result<InvoiceResponseDto>> GetInvoiceByNumberAsync(
        string invoiceNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _context.Invoices
                .Include(i => i.Tenant)
                .Include(i => i.Subscription)
                .Include(i => i.LineItems.OrderBy(li => li.LineNumber))
                    .ThenInclude(li => li.Module)
                .Include(i => i.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber, cancellationToken);

            if (invoice == null)
                return Result<InvoiceResponseDto>.Failure("Invoice not found", 404);

            var dto = _mapper.Map<InvoiceResponseDto>(invoice);

            return Result<InvoiceResponseDto>.Success(dto, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoice by number {InvoiceNumber}", invoiceNumber);
            return Result<InvoiceResponseDto>.Failure("An error occurred while getting invoice", 500);
        }
    }

    public async Task<Result<PaginatedResult<InvoiceResponseDto>>> GetInvoicesByTenantAsync(
        Guid tenantId,
        GetInvoicesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Invoices
                .Include(i => i.Tenant)
                .Include(i => i.Subscription)
                .Include(i => i.LineItems)
                .Include(i => i.Payments)
                .Where(i => i.TenantId == tenantId)
                .AsNoTracking();

            return await GetInvoicesInternalAsync(query, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoices for tenant {TenantId}", tenantId);
            return Result<PaginatedResult<InvoiceResponseDto>>.Failure("An error occurred while getting invoices", 500);
        }
    }

    public async Task<Result<PaginatedResult<InvoiceResponseDto>>> GetInvoicesAsync(
        GetInvoicesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Invoices
                .Include(i => i.Tenant)
                .Include(i => i.Subscription)
                .Include(i => i.LineItems)
                .Include(i => i.Payments)
                .AsNoTracking();

            return await GetInvoicesInternalAsync(query, request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoices");
            return Result<PaginatedResult<InvoiceResponseDto>>.Failure("An error occurred while getting invoices", 500);
        }
    }

    private async Task<Result<PaginatedResult<InvoiceResponseDto>>> GetInvoicesInternalAsync(
        IQueryable<Invoice> query,
        GetInvoicesRequest request,
        CancellationToken cancellationToken)
    {
        // Apply filters
        if (request.SubscriptionId.HasValue)
            query = query.Where(i => i.SubscriptionId == request.SubscriptionId.Value);

        if (request.Status.HasValue)
            query = query.Where(i => i.Status == request.Status.Value);

        if (request.Type.HasValue)
            query = query.Where(i => i.Type == request.Type.Value);

        if (request.IssueDateFrom.HasValue)
            query = query.Where(i => i.IssueDate >= request.IssueDateFrom.Value);

        if (request.IssueDateTo.HasValue)
            query = query.Where(i => i.IssueDate <= request.IssueDateTo.Value);

        if (request.DueDateFrom.HasValue)
            query = query.Where(i => i.DueDate >= request.DueDateFrom.Value);

        if (request.DueDateTo.HasValue)
            query = query.Where(i => i.DueDate <= request.DueDateTo.Value);

        if (request.IsOverdue.HasValue && request.IsOverdue.Value)
            query = query.Where(i => i.DueDate < DateTime.UtcNow && i.Status != InvoiceStatus.Paid);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(i =>
                i.InvoiceNumber.Contains(request.SearchTerm) ||
                i.Tenant.Name.Contains(request.SearchTerm));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy.ToLowerInvariant() switch
        {
            "invoicenumber" => request.SortDescending
                ? query.OrderByDescending(i => i.InvoiceNumber)
                : query.OrderBy(i => i.InvoiceNumber),
            "duedate" => request.SortDescending
                ? query.OrderByDescending(i => i.DueDate)
                : query.OrderBy(i => i.DueDate),
            "totalamount" => request.SortDescending
                ? query.OrderByDescending(i => i.TotalAmount)
                : query.OrderBy(i => i.TotalAmount),
            "status" => request.SortDescending
                ? query.OrderByDescending(i => i.Status)
                : query.OrderBy(i => i.Status),
            _ => request.SortDescending
                ? query.OrderByDescending(i => i.IssueDate)
                : query.OrderBy(i => i.IssueDate)
        };

        // Apply pagination
        var invoices = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<InvoiceResponseDto>>(invoices);

        var result = new PaginatedResult<InvoiceResponseDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.PageSize
        };

        return Result<PaginatedResult<InvoiceResponseDto>>.Success(result, 200);
    }

    public async Task<Result<bool>> SendInvoiceEmailAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invoiceResult = await GetInvoiceByIdAsync(invoiceId, cancellationToken);
            if (!invoiceResult.Succeeded || invoiceResult.Data == null)
                return Result<bool>.Failure(invoiceResult.Errors, invoiceResult.StatusCode);

            var invoice = invoiceResult.Data;

            // Build email
            var emailDto = new EmailDto
            {
                To = invoice.TenantName, // Should be actual email from tenant
                Subject = $"Invoice {invoice.InvoiceNumber} from Hotel Management System",
                Body = BuildInvoiceEmailBody(invoice),
                IsHtml = true
            };

            var sent = await _emailService.SendEmailAsync(emailDto, cancellationToken);

            if (sent)
            {
                // Update invoice sent timestamp
                var invoiceEntity = await _context.Invoices.FindAsync(invoiceId);
                if (invoiceEntity != null)
                {
                    invoiceEntity.SentAt = DateTime.UtcNow;
                    invoiceEntity.Status = InvoiceStatus.Sent;
                    await _context.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation("Sent invoice {InvoiceNumber} via email", invoice.InvoiceNumber);
            }

            return Result<bool>.Success(sent, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invoice email for {InvoiceId}", invoiceId);
            return Result<bool>.Failure("An error occurred while sending invoice email", 500);
        }
    }

    public async Task<Result<bool>> MarkInvoiceAsPaidAsync(
        Guid invoiceId,
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

            if (invoice == null)
                return Result<bool>.Failure("Invoice not found", 404);

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

            if (payment == null)
                return Result<bool>.Failure("Payment not found", 404);

            invoice.AmountPaid += payment.Amount;
            invoice.AmountDue = invoice.TotalAmount - invoice.AmountPaid;

            if (invoice.AmountDue <= 0)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidDate = DateTime.UtcNow;
            }
            else if (invoice.AmountPaid > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Marked invoice {InvoiceNumber} as paid with payment {PaymentReference}",
                invoice.InvoiceNumber, payment.PaymentReference);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking invoice {InvoiceId} as paid", invoiceId);
            return Result<bool>.Failure("An error occurred while marking invoice as paid", 500);
        }
    }

    public async Task<Result<PaymentResponseDto>> RecordManualPaymentAsync(
        RecordManualPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

            if (invoice == null)
                return Result<PaymentResponseDto>.Failure("Invoice not found", 404);

            // Generate payment reference
            var paymentCount = await _context.Payments.CountAsync(cancellationToken);
            var paymentReference = $"PAY-{DateTime.UtcNow.Year}-{(paymentCount + 1):D4}";

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                PaymentReference = paymentReference,
                InvoiceId = request.InvoiceId,
                TenantId = invoice.TenantId,
                SubscriptionId = invoice.SubscriptionId,
                Amount = request.Amount,
                Currency = invoice.Currency,
                Method = request.Method,
                Status = PaymentStatus.Completed,
                Gateway = PaymentGateway.Manual,
                CreatedAt = request.PaymentDate,
                ProcessedAt = request.PaymentDate,
                SettledAt = request.PaymentDate,
                GatewayTransactionId = request.TransactionReference,
                Notes = request.Notes,
                IsReconciled = false
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(cancellationToken);

            // Update invoice
            await MarkInvoiceAsPaidAsync(request.InvoiceId, payment.Id, cancellationToken);

            _logger.LogInformation("Recorded manual payment {PaymentReference} for invoice {InvoiceNumber}",
                paymentReference, invoice.InvoiceNumber);

            var dto = _mapper.Map<PaymentResponseDto>(payment);
            return Result<PaymentResponseDto>.Success(dto, 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording manual payment");
            return Result<PaymentResponseDto>.Failure("An error occurred while recording payment", 500);
        }
    }

    public async Task<Result<List<InvoiceResponseDto>>> GetOverdueInvoicesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invoices = await _context.Invoices
                .Include(i => i.Tenant)
                .Include(i => i.Subscription)
                .Include(i => i.LineItems)
                .Where(i => i.DueDate < DateTime.UtcNow)
                .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
                .OrderBy(i => i.DueDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<InvoiceResponseDto>>(invoices);

            _logger.LogInformation("Found {Count} overdue invoices", dtos.Count);

            return Result<List<InvoiceResponseDto>>.Success(dtos, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue invoices");
            return Result<List<InvoiceResponseDto>>.Failure("An error occurred while getting overdue invoices", 500);
        }
    }

    public async Task<Result<List<SubscriptionResponseDto>>> GetUpcomingInvoicesAsync(
        int daysAhead = 7,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var targetDate = DateTime.UtcNow.AddDays(daysAhead).Date;

            var subscriptions = await _context.Subscriptions
                .Include(s => s.Tenant)
                .Include(s => s.Plan)
                .Where(s => s.Status == SubscriptionStatus.Active)
                .Where(s => s.NextBillingDate.Date <= targetDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<SubscriptionResponseDto>>(subscriptions);

            return Result<List<SubscriptionResponseDto>>.Success(dtos, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming invoices");
            return Result<List<SubscriptionResponseDto>>.Failure("An error occurred while getting upcoming invoices", 500);
        }
    }

    public async Task<Result<bool>> CancelInvoiceAsync(
        Guid invoiceId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

            if (invoice == null)
                return Result<bool>.Failure("Invoice not found", 404);

            if (invoice.Status == InvoiceStatus.Paid)
                return Result<bool>.Failure("Cannot cancel a paid invoice", 400);

            invoice.Status = InvoiceStatus.Cancelled;
            invoice.Notes = $"Cancelled: {reason}. {invoice.Notes}";

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cancelled invoice {InvoiceNumber}. Reason: {Reason}",
                invoice.InvoiceNumber, reason);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling invoice {InvoiceId}", invoiceId);
            return Result<bool>.Failure("An error occurred while cancelling invoice", 500);
        }
    }

    public Task<Result<byte[]>> GenerateInvoicePdfAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement PDF generation using QuestPDF or similar library
        // This is a placeholder
        _logger.LogWarning("PDF generation not yet implemented for invoice {InvoiceId}", invoiceId);
        return Task.FromResult(Result<byte[]>.Failure("PDF generation not yet implemented", 501));
    }

    private string BuildInvoiceEmailBody(InvoiceResponseDto invoice)
    {
        return $@"
<html>
<body style=""font-family: Arial, sans-serif;"">
    <h2>Invoice {invoice.InvoiceNumber}</h2>
    <p>Dear {invoice.TenantName},</p>
    <p>Your invoice is ready for payment.</p>
    
    <table style=""width: 100%; border-collapse: collapse; margin: 20px 0;"">
        <tr>
            <td><strong>Invoice Number:</strong></td>
            <td>{invoice.InvoiceNumber}</td>
        </tr>
        <tr>
            <td><strong>Issue Date:</strong></td>
            <td>{invoice.IssueDate:yyyy-MM-dd}</td>
        </tr>
        <tr>
            <td><strong>Due Date:</strong></td>
            <td>{invoice.DueDate:yyyy-MM-dd}</td>
        </tr>
        <tr>
            <td><strong>Amount Due:</strong></td>
            <td><strong>{invoice.Currency} {invoice.TotalAmount:N2}</strong></td>
        </tr>
    </table>

    <p>Please make payment by {invoice.DueDate:yyyy-MM-dd}.</p>
    
    <p>Thank you for your business!</p>
    
    <p style=""color: #666; font-size: 12px;"">
        Hotel Management System<br/>
        Billing Department
    </p>
</body>
</html>";
    }
}

