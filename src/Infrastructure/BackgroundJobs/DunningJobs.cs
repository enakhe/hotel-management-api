using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.BackgroundJobs;

/// <summary>
/// Background jobs for dunning management (payment recovery)
/// </summary>
public class DunningJobs
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<DunningJobs> _logger;

    public DunningJobs(
        ApplicationDbContext context,
        IEmailService emailService,
        ISmsService smsService,
        ISubscriptionService subscriptionService,
        ILogger<DunningJobs> logger)
    {
        _context = context;
        _emailService = emailService;
        _smsService = smsService;
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    /// <summary>
    /// Process payment retries for failed invoices
    /// Runs daily
    /// </summary>
    public async Task ProcessPaymentRetriesAsync()
    {
        try
        {
            _logger.LogInformation("Starting payment retry processing...");

            // Get invoices that need retry
            var overdueInvoices = await _context.Invoices
                .Include(i => i.Tenant)
                .Include(i => i.Subscription)
                .Where(i => i.Status == InvoiceStatus.Overdue || i.Status == InvoiceStatus.Pending)
                .Where(i => i.DueDate < DateTime.UtcNow)
                .ToListAsync();

            foreach (var invoice in overdueInvoices)
            {
                // Check existing retry attempts
                var retryCount = await _context.PaymentRetries
                    .Where(pr => pr.InvoiceId == invoice.Id)
                    .CountAsync();

                var daysOverdue = (DateTime.UtcNow.Date - invoice.DueDate.Date).Days;

                // Retry schedule: Day 1, Day 3, Day 7, Day 14
                var shouldRetry = daysOverdue switch
                {
                    1 => retryCount == 0,
                    3 => retryCount == 1,
                    7 => retryCount == 2,
                    14 => retryCount == 3,
                    _ => false
                };

                if (shouldRetry)
                {
                    await CreatePaymentRetryAttemptAsync(invoice, retryCount + 1);
                }

                // Suspend after 14 days
                if (daysOverdue >= 14 && invoice.Subscription != null)
                {
                    if (invoice.Subscription.Status == SubscriptionStatus.Active)
                    {
                        await _subscriptionService.SuspendSubscriptionAsync(
                            invoice.Subscription.Id,
                            $"Payment overdue for {daysOverdue} days");
                    }
                }
            }

            _logger.LogInformation("Payment retry processing completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in payment retry processing");
            throw;
        }
    }

    private async Task CreatePaymentRetryAttemptAsync(Invoice invoice, int attemptNumber)
    {
        try
        {
            var retry = new PaymentRetry
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                TenantId = invoice.TenantId,
                AttemptNumber = attemptNumber,
                AttemptedAt = DateTime.UtcNow,
                NextRetryAt = CalculateNextRetryDate(attemptNumber),
                Status = PaymentRetryStatus.Attempted,
                NotificationSent = false
            };

            _context.PaymentRetries.Add(retry);
            await _context.SaveChangesAsync();

            // Send payment reminder
            await SendPaymentReminderAsync(invoice, attemptNumber);

            retry.NotificationSent = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created payment retry attempt {AttemptNumber} for invoice {InvoiceNumber}",
                attemptNumber, invoice.InvoiceNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment retry attempt");
        }
    }

    private DateTime CalculateNextRetryDate(int attemptNumber)
    {
        return attemptNumber switch
        {
            1 => DateTime.UtcNow.AddDays(2), // Retry in 2 days
            2 => DateTime.UtcNow.AddDays(4), // Retry in 4 days
            3 => DateTime.UtcNow.AddDays(7), // Retry in 7 days
            _ => DateTime.UtcNow.AddDays(14)
        };
    }

    private async Task SendPaymentReminderAsync(Invoice invoice, int attemptNumber)
    {
        try
        {
            var urgency = attemptNumber switch
            {
                1 => "Reminder",
                2 => "Second Notice",
                3 => "Final Notice",
                _ => "Urgent - Account Suspension Imminent"
            };

            var subject = $"{urgency}: Payment Due for Invoice {invoice.InvoiceNumber}";
            var body = $@"
<html>
<body style=""font-family: Arial, sans-serif;"">
    <h2 style=""color: #f44336;"">{urgency}</h2>
    <p>Dear {invoice.Tenant.Name},</p>
    <p>This is a {urgency.ToLower()} that payment for invoice <strong>{invoice.InvoiceNumber}</strong> is overdue.</p>
    
    <table style=""margin: 20px 0;"">
        <tr><td><strong>Invoice Number:</strong></td><td>{invoice.InvoiceNumber}</td></tr>
        <tr><td><strong>Amount Due:</strong></td><td>{invoice.Currency} {invoice.AmountDue:N2}</td></tr>
        <tr><td><strong>Due Date:</strong></td><td>{invoice.DueDate:yyyy-MM-dd}</td></tr>
        <tr><td><strong>Days Overdue:</strong></td><td>{(DateTime.UtcNow - invoice.DueDate).Days}</td></tr>
    </table>

    {(attemptNumber >= 3 ? "<p style=\"color: #f44336;\"><strong>WARNING:</strong> Your account will be suspended if payment is not received within 7 days.</p>" : "")}
    
    <p>Please make payment as soon as possible to avoid service interruption.</p>
    
    <p>Contact our billing department if you need assistance: billing@yourcompany.com</p>
</body>
</html>";

            var emailDto = new Application.Common.DTOs.EmailDto
            {
                To = invoice.Tenant.Email ?? "",
                Subject = subject,
                Body = body,
                IsHtml = true
            };

            await _emailService.SendEmailAsync(emailDto);

            // Send SMS for urgent notices
            if (attemptNumber >= 2 && !string.IsNullOrWhiteSpace(invoice.Tenant.ContactNumber))
            {
                var smsMessage = $"{urgency}: Invoice {invoice.InvoiceNumber} for {invoice.Currency} {invoice.AmountDue:N0} is overdue. Please pay immediately to avoid service suspension.";
                await _smsService.SendSmsAsync(invoice.Tenant.ContactNumber, smsMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending payment reminder");
        }
    }

    /// <summary>
    /// Send usage limit warnings when approaching limits
    /// Runs daily
    /// </summary>
    public Task SendUsageLimitWarningsAsync()
    {
        try
        {
            _logger.LogInformation("Checking usage limits for warnings...");

            // TODO: Implement usage limit warnings
            // Check tenants at 80%, 90%, 100% of their limits
            // Send notifications

            _logger.LogInformation("Usage limit warnings completed");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending usage limit warnings");
            throw;
        }
    }
}

