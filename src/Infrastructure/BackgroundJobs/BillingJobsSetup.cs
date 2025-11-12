using Hangfire;
using HotelManagement.Infrastructure.BackgroundJobs;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManagement.Infrastructure;

/// <summary>
/// Setup recurring jobs for billing automation
/// </summary>
public static class BillingJobsSetup
{
    public static void ConfigureBillingJobs()
    {
        // Register BillingJobs service
        RecurringJob.AddOrUpdate<BillingJobs>(
            "generate-monthly-invoices",
            job => job.GenerateMonthlyInvoicesAsync(),
            "0 2 * * *", // Daily at 2 AM
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time") // WAT
            });

        RecurringJob.AddOrUpdate<BillingJobs>(
            "send-invoice-reminders",
            job => job.SendInvoiceRemindersAsync(),
            "0 9 * * *", // Daily at 9 AM
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")
            });

        RecurringJob.AddOrUpdate<BillingJobs>(
            "process-overdue-invoices",
            job => job.ProcessOverdueInvoicesAsync(),
            "0 10 * * *", // Daily at 10 AM
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")
            });

        RecurringJob.AddOrUpdate<BillingJobs>(
            "update-subscription-status",
            job => job.UpdateSubscriptionStatusAsync(),
            "0 * * * *", // Hourly
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")
            });

        RecurringJob.AddOrUpdate<BillingJobs>(
            "calculate-usage",
            job => job.CalculateUsageAsync(),
            "0 * * * *", // Hourly
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")
            });

        // Notification jobs
        RecurringJob.AddOrUpdate<NotificationJobs>(
            "process-notification-queue",
            job => job.ProcessNotificationQueueAsync(),
            "*/5 * * * *", // Every 5 minutes
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")
            });

        RecurringJob.AddOrUpdate<NotificationJobs>(
            "retry-failed-notifications",
            job => job.RetryFailedNotificationsAsync(),
            "0 */6 * * *", // Every 6 hours
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")
            });

        RecurringJob.AddOrUpdate<NotificationJobs>(
            "cleanup-old-notifications",
            job => job.CleanupOldNotificationsAsync(),
            "0 3 * * *", // Daily at 3 AM
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")
            });

        // Dunning jobs
        RecurringJob.AddOrUpdate<DunningJobs>(
            "process-payment-retries",
            job => job.ProcessPaymentRetriesAsync(),
            "0 11 * * *", // Daily at 11 AM
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")
            });

        RecurringJob.AddOrUpdate<DunningJobs>(
            "send-usage-limit-warnings",
            job => job.SendUsageLimitWarningsAsync(),
            "0 8 * * *", // Daily at 8 AM
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Central Africa Standard Time")
            });
    }
}

