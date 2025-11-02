using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Reports.Generators;

/// <summary>
/// Generator for financial reports
/// </summary>
public class FinancialReportGenerator : IReportDataGenerator
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<FinancialReportGenerator> _logger;

    public ReportType ReportType => ReportType.Financial;

    public FinancialReportGenerator(
        IApplicationDbContext context,
        ILogger<FinancialReportGenerator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ReportDataDto> GenerateAsync(ReportParametersDto parameters, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Financial Report");

        var startDate = parameters.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = parameters.EndDate ?? DateTime.UtcNow;

        // Generate sections
        var revenueSection = await GenerateRevenueByPlanAsync(startDate, endDate, cancellationToken);
        var licenseSalesSection = await GenerateLicenseSalesAsync(startDate, endDate, cancellationToken);
        var tenantGrowthSection = await GenerateTenantGrowthAsync(startDate, endDate, cancellationToken);

        var sections = new List<ReportSectionDto>
        {
            revenueSection,
            licenseSalesSection,
            tenantGrowthSection
        };

        // Calculate summary
        var totalRevenue = await CalculateTotalRevenueAsync(startDate, endDate, cancellationToken);
        var activeTenantCount = await _context.Tenants.CountAsync(t => t.IsActive, cancellationToken);
        var newTenantsInPeriod = await _context.Tenants.CountAsync(t => t.Created >= startDate && t.Created <= endDate, cancellationToken);

        var reportData = new ReportDataDto
        {
            Title = "Financial Report",
            Description = $"Financial overview from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            Type = ReportType.Financial,
            GeneratedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "StartDate", startDate },
                { "EndDate", endDate },
                { "Period", $"{(endDate - startDate).Days} days" }
            },
            Sections = sections,
            Summary = new ReportSummaryDto
            {
                TotalRecords = sections.Sum(s => s.Tables.Sum(t => t.Rows.Count)),
                Statistics = new Dictionary<string, object>
                {
                    { "TotalRevenue", totalRevenue },
                    { "AverageRevenuePerTenant", totalRevenue / Math.Max(1, activeTenantCount) },
                    { "NewTenantsInPeriod", newTenantsInPeriod }
                }
            }
        };

        return reportData;
    }

    private async Task<ReportSectionDto> GenerateRevenueByPlanAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var revenueByPlan = await _context.Plans
            .Include(p => p.Tenants)
            .Where(p => p.Tenants.Any(t => t.Created >= startDate && t.Created <= endDate))
            .Select(p => new
            {
                p.Name,
                TenantsCount = p.Tenants.Count(t => t.Created >= startDate && t.Created <= endDate && t.IsActive)
            })
            .ToListAsync(cancellationToken);

        var section = new ReportSectionDto
        {
            Title = "Revenue by Plan",
            Description = "Subscription revenue breakdown by plan",
            Tables = new List<ReportTableDto>
            {
                new ReportTableDto
                {
                    Title = "Plan Revenue",
                    Columns = new List<string> { "Plan Name", "New Subscriptions", "Estimated Monthly Revenue" },
                    Rows = revenueByPlan.Select(r => new List<object> { r.Name, r.TenantsCount, r.TenantsCount * 100 }).ToList()
                }
            },
            Charts = new List<ReportChartDto>
            {
                new ReportChartDto
                {
                    Title = "Revenue Distribution",
                    ChartType = "pie",
                    Labels = revenueByPlan.Select(r => r.Name).ToList(),
                    Datasets = new List<ReportDatasetDto>
                    {
                        new ReportDatasetDto
                        {
                            Label = "Revenue",
                            Data = revenueByPlan.Select(r => (object)(r.TenantsCount * 100)).ToList()
                        }
                    }
                }
            }
        };

        return section;
    }

    private async Task<ReportSectionDto> GenerateLicenseSalesAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var licenseSales = await _context.Licenses
            .Where(l => l.IssuedDate >= startDate && l.IssuedDate <= endDate)
            .GroupBy(l => l.Type)
            .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
            .ToListAsync(cancellationToken);

        var section = new ReportSectionDto
        {
            Title = "License Sales",
            Description = "License sales breakdown by type",
            Tables = new List<ReportTableDto>
            {
                new ReportTableDto
                {
                    Title = "Sales by License Type",
                    Columns = new List<string> { "License Type", "Count" },
                    Rows = licenseSales.Select(l => new List<object> { l.Type, l.Count }).ToList()
                }
            }
        };

        return section;
    }

    private async Task<ReportSectionDto> GenerateTenantGrowthAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var tenantGrowth = await _context.Tenants
            .Where(t => t.Created >= startDate && t.Created <= endDate)
            .GroupBy(t => new { t.Created.Year, t.Created.Month })
            .Select(g => new { 
                Year = g.Key.Year, 
                Month = g.Key.Month, 
                Count = g.Count() 
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(cancellationToken);

        var section = new ReportSectionDto
        {
            Title = "Tenant Growth",
            Description = "Tenant acquisition trend over time",
            Tables = new List<ReportTableDto>
            {
                new ReportTableDto
                {
                    Title = "Monthly Tenant Growth",
                    Columns = new List<string> { "Period", "New Tenants" },
                    Rows = tenantGrowth.Select(g => new List<object> { $"{g.Year}-{g.Month:D2}", g.Count }).ToList()
                }
            },
            Charts = new List<ReportChartDto>
            {
                new ReportChartDto
                {
                    Title = "Tenant Growth Trend",
                    ChartType = "line",
                    Labels = tenantGrowth.Select(g => $"{g.Year}-{g.Month:D2}").ToList(),
                    Datasets = new List<ReportDatasetDto>
                    {
                        new ReportDatasetDto
                        {
                            Label = "New Tenants",
                            Data = tenantGrowth.Select(g => (object)g.Count).ToList(),
                            Color = "#FF9800"
                        }
                    }
                }
            }
        };

        return section;
    }

    private async Task<double> CalculateTotalRevenueAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var newTenants = await _context.Tenants.CountAsync(t => t.Created >= startDate && t.Created <= endDate && t.IsActive, cancellationToken);
        return newTenants * 100; // Placeholder calculation
    }

    public Task<bool> ValidateParametersAsync(ReportParametersDto parameters)
    {
        if (parameters.StartDate.HasValue && parameters.EndDate.HasValue && parameters.StartDate > parameters.EndDate)
        {
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }

    public ReportParametersDto GetDefaultParameters()
    {
        return new ReportParametersDto
        {
            StartDate = DateTime.UtcNow.AddMonths(-1),
            EndDate = DateTime.UtcNow,
            IncludeInactive = false
        };
    }
}

