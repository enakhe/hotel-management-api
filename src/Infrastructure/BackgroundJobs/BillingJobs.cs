using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.BackgroundJobs;

/// <summary>
/// Background jobs for automated billing operations
/// </summary>
public class BillingJobs
{
    private readonly ApplicationDbContext _context;
    private readonly IInvoicingService _invoicingService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IUsageTrackingService _usageTrackingService;
    private readonly ILogger<BillingJobs> _logger;

    public BillingJobs(
        ApplicationDbContext context,
        IInvoicingService invoicingService,
        ISubscriptionService subscriptionService,
        IUsageTrackingService usageTrackingService,
        ILogger<BillingJobs> logger)
    {
        _context = context;
        _invoicingService = invoicingService;
        _subscriptionService = subscriptionService;
        _usageTrackingService = usageTrackingService;
        _logger = logger;
    }

    /// <summary>
    /// Generate monthly invoices for active subscriptions
    /// Runs daily at 2 AM
    /// </summary>
    public async Task GenerateMonthlyInvoicesAsync()
    {
        try
        {
            _logger.LogInformation("Starting monthly invoice generation job...");

            // Get subscriptions due for billing today
            var today = DateTime.UtcNow.Date;
            var subscriptionsDue = await _subscriptionService.GetSubscriptionsDueForRenewalAsync(today);

            if (!subscriptionsDue.Succeeded)
            {
                _logger.LogError("Error getting subscriptions due for renewal: {Error}", string.Join(", ", subscriptionsDue.Errors));
                return;
            }

            var count = 0;
            foreach (var subscription in subscriptionsDue.Data ?? new List<SubscriptionResponseDto>())
            {
                try
                {
                    _logger.LogInformation("Generating invoice for subscription {SubscriptionNumber}",
                        subscription.SubscriptionNumber);

                    // Generate invoice
                    var invoiceResult = await _invoicingService.GenerateInvoiceAsync(
                        subscription.Id,
                        InvoiceType.Recurring);

                    if (invoiceResult.Succeeded && invoiceResult.Data != null)
                    {
                        // Send invoice email
                        await _invoicingService.SendInvoiceEmailAsync(invoiceResult.Data.Id);

                        // Update subscription next billing date
                        var updateResult = await _subscriptionService.UpdateSubscriptionAsync(
                            subscription.Id,
                            new UpdateSubscriptionRequest
                            {
                                NextBillingDate = subscription.NextBillingDate.AddMonths(1)
                            });

                        count++;
                        _logger.LogInformation("Invoice {InvoiceNumber} generated and sent for subscription {SubscriptionNumber}",
                            invoiceResult.Data.InvoiceNumber, subscription.SubscriptionNumber);
                    }
                    else
                    {
                        _logger.LogError("Failed to generate invoice for subscription {SubscriptionNumber}: {Error}",
                            subscription.SubscriptionNumber, string.Join(", ", invoiceResult.Errors));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing subscription {SubscriptionId}", subscription.Id);
                }
            }

            _logger.LogInformation("Monthly invoice generation completed. Generated {Count} invoices", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in monthly invoice generation job");
            throw;
        }
    }

    /// <summary>
    /// Send invoice reminders for unpaid invoices
    /// Runs daily at 9 AM
    /// </summary>
    public async Task SendInvoiceRemindersAsync()
    {
        try
        {
            _logger.LogInformation("Starting invoice reminder job...");

            // Get invoices due in 3 days or already overdue
            var reminderDate = DateTime.UtcNow.AddDays(3).Date;

            var invoices = await _context.Invoices
                .Include(i => i.Tenant)
                .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
                .Where(i => i.DueDate.Date <= reminderDate)
                .Where(i => !i.SentAt.HasValue || i.SentAt.Value < DateTime.UtcNow.AddDays(-7)) // Don't spam
                .ToListAsync();

            var count = 0;
            foreach (var invoice in invoices)
            {
                try
                {
                    await _invoicingService.SendInvoiceEmailAsync(invoice.Id);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending reminder for invoice {InvoiceNumber}", invoice.InvoiceNumber);
                }
            }

            _logger.LogInformation("Invoice reminder job completed. Sent {Count} reminders", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in invoice reminder job");
            throw;
        }
    }

    /// <summary>
    /// Process overdue invoices and update subscription status
    /// Runs daily at 10 AM
    /// </summary>
    public async Task ProcessOverdueInvoicesAsync()
    {
        try
        {
            _logger.LogInformation("Starting overdue invoice processing job...");

            var overdueInvoices = await _context.Invoices
                .Include(i => i.Subscription)
                .Where(i => i.Status != InvoiceStatus.Paid)
                .Where(i => i.Status != InvoiceStatus.Cancelled)
                .Where(i => i.DueDate.Date < DateTime.UtcNow.Date)
                .ToListAsync();

            var count = 0;
            foreach (var invoice in overdueInvoices)
            {
                // Update invoice status
                if (invoice.Status != InvoiceStatus.Overdue)
                {
                    invoice.Status = InvoiceStatus.Overdue;
                    count++;
                }

                // Suspend subscription if invoice is more than 7 days overdue
                var daysOverdue = (DateTime.UtcNow.Date - invoice.DueDate.Date).Days;
                if (daysOverdue > 7 && invoice.Subscription != null)
                {
                    if (invoice.Subscription.Status == SubscriptionStatus.Active)
                    {
                        await _subscriptionService.SuspendSubscriptionAsync(
                            invoice.Subscription.Id,
                            $"Payment overdue by {daysOverdue} days");

                        _logger.LogWarning("Suspended subscription {SubscriptionNumber} due to overdue invoice {InvoiceNumber}",
                            invoice.Subscription.SubscriptionNumber, invoice.InvoiceNumber);
                    }
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Overdue invoice processing completed. Updated {Count} invoices", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in overdue invoice processing job");
            throw;
        }
    }

    /// <summary>
    /// Update subscription statuses based on current state
    /// Runs hourly
    /// </summary>
    public async Task UpdateSubscriptionStatusAsync()
    {
        try
        {
            _logger.LogInformation("Starting subscription status update job...");

            // End trials that have expired
            var expiredTrials = await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Trial)
                .Where(s => s.TrialEndDate.HasValue && s.TrialEndDate.Value < DateTime.UtcNow)
                .ToListAsync();

            foreach (var subscription in expiredTrials)
            {
                subscription.Status = SubscriptionStatus.PendingPayment;
                _logger.LogInformation("Trial expired for subscription {SubscriptionNumber}", subscription.SubscriptionNumber);
            }

            // Mark expired subscriptions
            var expiredSubscriptions = await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active)
                .Where(s => s.EndDate.HasValue && s.EndDate.Value < DateTime.UtcNow)
                .ToListAsync();

            foreach (var subscription in expiredSubscriptions)
            {
                subscription.Status = SubscriptionStatus.Expired;
                _logger.LogInformation("Subscription expired: {SubscriptionNumber}", subscription.SubscriptionNumber);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Subscription status update completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in subscription status update job");
            throw;
        }
    }

    /// <summary>
    /// Aggregate usage records for all active subscriptions
    /// Runs hourly
    /// </summary>
    public async Task CalculateUsageAsync()
    {
        try
        {
            _logger.LogInformation("Starting usage calculation job...");

            var activeSubscriptions = await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active)
                .ToListAsync();

            foreach (var subscription in activeSubscriptions)
            {
                try
                {
                    var periodStart = subscription.LastBillingDate ?? subscription.StartDate;
                    var periodEnd = subscription.NextBillingDate;

                    // Aggregate usage for this subscription
                    await _usageTrackingService.AggregateUsageAsync(
                        subscription.Id,
                        periodStart,
                        periodEnd);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating usage for subscription {SubscriptionId}", subscription.Id);
                }
            }

            _logger.LogInformation("Usage calculation job completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in usage calculation job");
            throw;
        }
    }
}

