using Asp.Versioning;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Security;
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Web.Controllers.SuperAdmin;

/// <summary>
/// SuperAdmin billing dashboard and analytics controller
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("cp/api/v{version:apiVersion}/billing/dashboard")]
[Authorize(Roles = "SuperAdministrator")]
[Produces("application/json")]
public class BillingDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IInvoicingService _invoicingService;
    private readonly ILogger<BillingDashboardController> _logger;

    public BillingDashboardController(
        ApplicationDbContext context,
        ISubscriptionService subscriptionService,
        IInvoicingService invoicingService,
        ILogger<BillingDashboardController> logger)
    {
        _context = context;
        _subscriptionService = subscriptionService;
        _invoicingService = invoicingService;
        _logger = logger;
    }

    /// <summary>
    /// Get billing dashboard statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(BillingDashboardStats), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            var activeSubscriptions = await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active)
                .CountAsync();

            var trialSubscriptions = await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Trial)
                .CountAsync();

            var cancelledSubscriptions = await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Cancelled)
                .CountAsync();

            var suspendedSubscriptions = await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Suspended)
                .CountAsync();

            // Calculate MRR (Monthly Recurring Revenue)
            var mrr = await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active)
                .SumAsync(s => s.MonthlyPrice);

            // Calculate ARR (Annual Recurring Revenue)
            var arr = mrr * 12;

            // Outstanding invoices
            var outstandingInvoices = await _context.Invoices
                .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
                .CountAsync();

            var outstandingAmount = await _context.Invoices
                .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
                .SumAsync(i => i.AmountDue);

            // Overdue invoices
            var overdueInvoices = await _context.Invoices
                .Where(i => i.DueDate < DateTime.UtcNow)
                .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
                .CountAsync();

            var overdueAmount = await _context.Invoices
                .Where(i => i.DueDate < DateTime.UtcNow)
                .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Cancelled)
                .SumAsync(i => i.AmountDue);

            // This month's revenue
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var monthlyRevenue = await _context.Payments
                .Where(p => p.ProcessedAt >= startOfMonth)
                .Where(p => p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount);

            // Payment success rate this month
            var totalPaymentsThisMonth = await _context.Payments
                .Where(p => p.CreatedAt >= startOfMonth)
                .CountAsync();

            var successfulPaymentsThisMonth = await _context.Payments
                .Where(p => p.CreatedAt >= startOfMonth)
                .Where(p => p.Status == PaymentStatus.Completed)
                .CountAsync();

            var paymentSuccessRate = totalPaymentsThisMonth > 0
                ? (decimal)successfulPaymentsThisMonth / totalPaymentsThisMonth * 100
                : 0;

            var stats = new BillingDashboardStats
            {
                ActiveSubscriptions = activeSubscriptions,
                TrialSubscriptions = trialSubscriptions,
                CancelledSubscriptions = cancelledSubscriptions,
                SuspendedSubscriptions = suspendedSubscriptions,
                TotalSubscriptions = activeSubscriptions + trialSubscriptions + cancelledSubscriptions + suspendedSubscriptions,
                MonthlyRecurringRevenue = mrr,
                AnnualRecurringRevenue = arr,
                OutstandingInvoices = outstandingInvoices,
                OutstandingAmount = outstandingAmount,
                OverdueInvoices = overdueInvoices,
                OverdueAmount = overdueAmount,
                MonthlyRevenue = monthlyRevenue,
                PaymentSuccessRate = paymentSuccessRate,
                Currency = "NGN" // Default currency
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing dashboard stats");
            return StatusCode(500, "An error occurred while getting dashboard statistics");
        }
    }

    /// <summary>
    /// Get revenue over time
    /// </summary>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(List<RevenueDataPoint>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueData([FromQuery] int months = 12)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddMonths(-months).Date;
            var revenue = await _context.Payments
                .Where(p => p.ProcessedAt >= startDate)
                .Where(p => p.Status == PaymentStatus.Completed)
                .GroupBy(p => new 
                { 
                    Year = p.ProcessedAt!.Value.Year, 
                    Month = p.ProcessedAt!.Value.Month 
                })
                .Select(g => new RevenueDataPoint
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(p => p.Amount),
                    Count = g.Count()
                })
                .OrderBy(r => r.Year)
                .ThenBy(r => r.Month)
                .ToListAsync();

            return Ok(revenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue data");
            return StatusCode(500, "An error occurred while getting revenue data");
        }
    }

    /// <summary>
    /// Get subscription trends
    /// </summary>
    [HttpGet("subscription-trends")]
    [ProducesResponseType(typeof(List<SubscriptionTrendDataPoint>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionTrends([FromQuery] int months = 12)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddMonths(-months).Date;
            
            var subscriptionTrends = await _context.Subscriptions
                .Where(s => s.StartDate >= startDate)
                .GroupBy(s => new 
                { 
                    Year = s.StartDate.Year, 
                    Month = s.StartDate.Month 
                })
                .Select(g => new SubscriptionTrendDataPoint
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    NewSubscriptions = g.Count(),
                    ActiveSubscriptions = g.Count(s => s.Status == SubscriptionStatus.Active),
                    CancelledSubscriptions = g.Count(s => s.Status == SubscriptionStatus.Cancelled)
                })
                .OrderBy(t => t.Year)
                .ThenBy(t => t.Month)
                .ToListAsync();

            return Ok(subscriptionTrends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription trends");
            return StatusCode(500, "An error occurred while getting subscription trends");
        }
    }

    /// <summary>
    /// Get plan distribution
    /// </summary>
    [HttpGet("plan-distribution")]
    [ProducesResponseType(typeof(List<PlanDistributionDataPoint>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlanDistribution()
    {
        try
        {
            var distribution = await _context.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial)
                .GroupBy(s => new { s.PlanId, s.Plan.Name })
                .Select(g => new PlanDistributionDataPoint
                {
                    PlanId = g.Key.PlanId,
                    PlanName = g.Key.Name,
                    SubscriptionCount = g.Count(),
                    TotalRevenue = g.Sum(s => s.MonthlyPrice)
                })
                .OrderByDescending(d => d.SubscriptionCount)
                .ToListAsync();

            return Ok(distribution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting plan distribution");
            return StatusCode(500, "An error occurred while getting plan distribution");
        }
    }
}

/// <summary>
/// Billing dashboard statistics
/// </summary>
public record BillingDashboardStats
{
    public int ActiveSubscriptions { get; init; }
    public int TrialSubscriptions { get; init; }
    public int CancelledSubscriptions { get; init; }
    public int SuspendedSubscriptions { get; init; }
    public int TotalSubscriptions { get; init; }
    public decimal MonthlyRecurringRevenue { get; init; }
    public decimal AnnualRecurringRevenue { get; init; }
    public int OutstandingInvoices { get; init; }
    public decimal OutstandingAmount { get; init; }
    public int OverdueInvoices { get; init; }
    public decimal OverdueAmount { get; init; }
    public decimal MonthlyRevenue { get; init; }
    public decimal PaymentSuccessRate { get; init; }
    public string Currency { get; init; } = "NGN";
}

/// <summary>
/// Revenue data point for charts
/// </summary>
public record RevenueDataPoint
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal Amount { get; init; }
    public int Count { get; init; }
}

/// <summary>
/// Subscription trend data point
/// </summary>
public record SubscriptionTrendDataPoint
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int NewSubscriptions { get; init; }
    public int ActiveSubscriptions { get; init; }
    public int CancelledSubscriptions { get; init; }
}

/// <summary>
/// Plan distribution data point
/// </summary>
public record PlanDistributionDataPoint
{
    public Guid PlanId { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public int SubscriptionCount { get; init; }
    public decimal TotalRevenue { get; init; }
}

