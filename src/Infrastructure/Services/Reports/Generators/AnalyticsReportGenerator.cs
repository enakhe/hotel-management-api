using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Reports.Generators;

/// <summary>
/// Generator for analytics reports
/// </summary>
public class AnalyticsReportGenerator : IReportDataGenerator
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AnalyticsReportGenerator> _logger;

    public ReportType ReportType => ReportType.Analytics;

    public AnalyticsReportGenerator(
        IApplicationDbContext context,
        ILogger<AnalyticsReportGenerator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ReportDataDto> GenerateAsync(ReportParametersDto parameters, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Analytics Report");

        var startDate = parameters.StartDate ?? DateTime.UtcNow.AddMonths(-3);
        var endDate = parameters.EndDate ?? DateTime.UtcNow;

        // Generate sections
        var userGrowthSection = await GenerateUserGrowthTrendsAsync(startDate, endDate, cancellationToken);
        var moduleAdoptionSection = await GenerateModuleAdoptionAsync(cancellationToken);
        var licenseUtilizationSection = await GenerateLicenseUtilizationAsync(cancellationToken);

        var sections = new List<ReportSectionDto>
        {
            userGrowthSection,
            moduleAdoptionSection,
            licenseUtilizationSection
        };

        var reportData = new ReportDataDto
        {
            Title = "Analytics Report",
            Description = $"System analytics and trends from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            Type = ReportType.Analytics,
            GeneratedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "StartDate", startDate },
                { "EndDate", endDate }
            },
            Sections = sections,
            Summary = new ReportSummaryDto
            {
                TotalRecords = sections.Sum(s => s.Tables.Sum(t => t.Rows.Count)),
                Statistics = new Dictionary<string, object>
                {
                    { "AnalysisPeriod", $"{(endDate - startDate).Days} days" },
                    { "DataPoints", sections.Count }
                }
            }
        };

        return reportData;
    }

    private async Task<ReportSectionDto> GenerateUserGrowthTrendsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var userGrowth = await _context.Users
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count()
            })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(cancellationToken);

        var section = new ReportSectionDto
        {
            Title = "User Growth Trends",
            Description = "User acquisition trend across all tenants",
            Tables = new List<ReportTableDto>
            {
                new ReportTableDto
                {
                    Title = "Monthly User Growth",
                    Columns = new List<string> { "Period", "New Users" },
                    Rows = userGrowth.Select(g => new List<object> { $"{g.Year}-{g.Month:D2}", g.Count }).ToList()
                }
            },
            Charts = new List<ReportChartDto>
            {
                new ReportChartDto
                {
                    Title = "User Growth Trend",
                    ChartType = "line",
                    Labels = userGrowth.Select(g => $"{g.Year}-{g.Month:D2}").ToList(),
                    Datasets = new List<ReportDatasetDto>
                    {
                        new ReportDatasetDto
                        {
                            Label = "New Users",
                            Data = userGrowth.Select(g => (object)g.Count).ToList(),
                            Color = "#3F51B5"
                        }
                    }
                }
            }
        };

        return section;
    }

    private async Task<ReportSectionDto> GenerateModuleAdoptionAsync(CancellationToken cancellationToken)
    {
        var moduleAdoption = await _context.Modules
            .Include(m => m.PlanModules)
            .Select(m => new
            {
                m.Name,
                PlansCount = m.PlanModules.Count,
                m.IsActive
            })
            .ToListAsync(cancellationToken);

        var section = new ReportSectionDto
        {
            Title = "Module Adoption Rates",
            Description = "Module usage across different plans",
            Tables = new List<ReportTableDto>
            {
                new ReportTableDto
                {
                    Title = "Module Adoption",
                    Columns = new List<string> { "Module", "Plans Using", "Status" },
                    Rows = moduleAdoption.Select(m => new List<object> 
                    { 
                        m.Name, 
                        m.PlansCount, 
                        m.IsActive ? "Active" : "Inactive" 
                    }).ToList()
                }
            }
        };

        return section;
    }

    private async Task<ReportSectionDto> GenerateLicenseUtilizationAsync(CancellationToken cancellationToken)
    {
        var totalLicenses = await _context.Licenses.CountAsync(cancellationToken);
        var activeLicenses = await _context.Licenses.CountAsync(l => l.Status == LicenseStatusType.Active, cancellationToken);
        var utilizationRate = totalLicenses > 0 ? (activeLicenses * 100.0 / totalLicenses) : 0;

        var section = new ReportSectionDto
        {
            Title = "License Utilization",
            Description = "License usage and efficiency metrics",
            Tables = new List<ReportTableDto>
            {
                new ReportTableDto
                {
                    Title = "Utilization Metrics",
                    Columns = new List<string> { "Metric", "Value" },
                    Rows = new List<List<object>>
                    {
                        new List<object> { "Total Licenses", totalLicenses },
                        new List<object> { "Active Licenses", activeLicenses },
                        new List<object> { "Utilization Rate", $"{utilizationRate:F2}%" }
                    }
                }
            }
        };

        return section;
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
            StartDate = DateTime.UtcNow.AddMonths(-3),
            EndDate = DateTime.UtcNow
        };
    }
}

