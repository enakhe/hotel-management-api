using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Reports.Generators;

/// <summary>
/// Generator for system overview reports
/// </summary>
public class SystemOverviewReportGenerator : IReportDataGenerator
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SystemOverviewReportGenerator> _logger;

    public ReportType ReportType => ReportType.SystemOverview;

    public SystemOverviewReportGenerator(
        IApplicationDbContext context,
        ILogger<SystemOverviewReportGenerator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ReportDataDto> GenerateAsync(ReportParametersDto parameters, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating System Overview Report");

        // Generate sections
        var tenantSection = await GenerateTenantStatisticsAsync(parameters, cancellationToken);
        var licenseSection = await GenerateLicenseStatisticsAsync(parameters, cancellationToken);
        var planSection = await GeneratePlanStatisticsAsync(parameters, cancellationToken);
        var moduleSection = await GenerateModuleStatisticsAsync(parameters, cancellationToken);

        var sections = new List<ReportSectionDto>
        {
            tenantSection,
            licenseSection,
            planSection,
            moduleSection
        };

        // Calculate summary
        var totalTenants = await _context.Tenants.CountAsync(cancellationToken);
        var activeTenants = await _context.Tenants.CountAsync(t => t.IsActive, cancellationToken);
        var totalLicenses = await _context.Licenses.CountAsync(cancellationToken);
        var totalPlans = await _context.Plans.CountAsync(cancellationToken);

        var reportData = new ReportDataDto
        {
            Title = "System Overview Report",
            Description = "Comprehensive overview of the system status",
            Type = ReportType.SystemOverview,
            GeneratedAt = DateTime.UtcNow,
            Sections = sections,
            Summary = new ReportSummaryDto
            {
                TotalRecords = sections.Sum(s => s.Tables.Sum(t => t.Rows.Count)),
                Statistics = new Dictionary<string, object>
                {
                    { "TotalTenants", totalTenants },
                    { "ActiveTenants", activeTenants },
                    { "TotalLicenses", totalLicenses },
                    { "TotalPlans", totalPlans }
                }
            }
        };

        return reportData;
    }

    private async Task<ReportSectionDto> GenerateTenantStatisticsAsync(ReportParametersDto parameters, CancellationToken cancellationToken)
    {
        var query = _context.Tenants.AsQueryable();

        if (!parameters.IncludeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        var totalTenants = await query.CountAsync(cancellationToken);
        var activeTenants = await query.CountAsync(t => t.IsActive, cancellationToken);
        var inactiveTenants = totalTenants - activeTenants;

        var tenantsByRegion = await query
            .GroupBy(t => t.Region)
            .Select(g => new { Region = g.Key ?? "Unknown", Count = g.Count() })
            .ToListAsync(cancellationToken);

        var tenantsByPlan = await query
            .Include(t => t.Plan)
            .GroupBy(t => t.Plan.Name)
            .Select(g => new { Plan = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var section = new ReportSectionDto
        {
            Title = "Tenant Statistics",
            Description = "Overview of tenant distribution and status",
            Tables = new List<ReportTableDto>
            {
                new ReportTableDto
                {
                    Title = "Tenant Status",
                    Columns = new List<string> { "Status", "Count", "Percentage" },
                    Rows = new List<List<object>>
                    {
                        new List<object> { "Active", activeTenants, $"{(totalTenants > 0 ? (activeTenants * 100.0 / totalTenants):0):F2}%" },
                        new List<object> { "Inactive", inactiveTenants, $"{(totalTenants > 0 ? (inactiveTenants * 100.0 / totalTenants):0):F2}%" }
                    }
                },
                new ReportTableDto
                {
                    Title = "Tenants by Region",
                    Columns = new List<string> { "Region", "Count" },
                    Rows = tenantsByRegion.Select(r => new List<object> { r.Region, r.Count }).ToList()
                },
                new ReportTableDto
                {
                    Title = "Tenants by Plan",
                    Columns = new List<string> { "Plan", "Count" },
                    Rows = tenantsByPlan.Select(p => new List<object> { p.Plan, p.Count }).ToList()
                }
            },
            Charts = new List<ReportChartDto>
            {
                new ReportChartDto
                {
                    Title = "Tenant Distribution by Region",
                    ChartType = "bar",
                    Labels = tenantsByRegion.Select(r => r.Region).ToList(),
                    Datasets = new List<ReportDatasetDto>
                    {
                        new ReportDatasetDto
                        {
                            Label = "Tenants",
                            Data = tenantsByRegion.Select(r => (object)r.Count).ToList(),
                            Color = "#4CAF50"
                        }
                    }
                }
            }
        };

        return section;
    }

    private async Task<ReportSectionDto> GenerateLicenseStatisticsAsync(ReportParametersDto parameters, CancellationToken cancellationToken)
    {
        var totalLicenses = await _context.Licenses.CountAsync(cancellationToken);
        var activeLicenses = await _context.Licenses.CountAsync(l => l.Status == LicenseStatusType.Active, cancellationToken);
        var expiredLicenses = await _context.Licenses.CountAsync(l => l.Status == LicenseStatusType.Expired, cancellationToken);
        var expiringLicenses = await _context.Licenses.CountAsync(l => l.ExpirationDate <= DateTime.UtcNow.AddDays(30) && l.Status == LicenseStatusType.Active, cancellationToken);

        var section = new ReportSectionDto
        {
            Title = "License Statistics",
            Description = "Overview of license status and distribution",
            Tables = new List<ReportTableDto>
            {
                new ReportTableDto
                {
                    Title = "License Status",
                    Columns = new List<string> { "Status", "Count", "Percentage" },
                    Rows = new List<List<object>>
                    {
                        new List<object> { "Active", activeLicenses, $"{(totalLicenses > 0 ? (activeLicenses * 100.0 / totalLicenses):0):F2}%" },
                        new List<object> { "Expired", expiredLicenses, $"{(totalLicenses > 0 ? (expiredLicenses * 100.0 / totalLicenses):0):F2}%" },
                        new List<object> { "Expiring Soon (30 days)", expiringLicenses, $"{(totalLicenses > 0 ? (expiringLicenses * 100.0 / totalLicenses):0):F2}%" }
                    }
                }
            },
            Charts = new List<ReportChartDto>
            {
                new ReportChartDto
                {
                    Title = "License Status Distribution",
                    ChartType = "pie",
                    Labels = new List<string> { "Active", "Expired", "Expiring Soon" },
                    Datasets = new List<ReportDatasetDto>
                    {
                        new ReportDatasetDto
                        {
                            Label = "Licenses",
                            Data = new List<object> { activeLicenses, expiredLicenses, expiringLicenses },
                            Color = "#2196F3"
                        }
                    }
                }
            }
        };

        return section;
    }

    private async Task<ReportSectionDto> GeneratePlanStatisticsAsync(ReportParametersDto parameters, CancellationToken cancellationToken)
    {
        var totalPlans = await _context.Plans.CountAsync(cancellationToken);
        var activePlans = await _context.Plans.CountAsync(p => p.IsActive, cancellationToken);

        var planUsage = await _context.Plans
            .Select(p => new
            {
                p.Name,
                TenantsCount = p.Tenants.Count,
                LicensesCount = p.Licenses.Count
            })
            .ToListAsync(cancellationToken);

        var section = new ReportSectionDto
        {
            Title = "Plan Statistics",
            Description = "Overview of subscription plans and usage",
            Tables = new List<ReportTableDto>
            {
                new ReportTableDto
                {
                    Title = "Plan Usage",
                    Columns = new List<string> { "Plan Name", "Tenants", "Licenses" },
                    Rows = planUsage.Select(p => new List<object> { p.Name, p.TenantsCount, p.LicensesCount }).ToList()
                }
            }
        };

        return section;
    }

    private async Task<ReportSectionDto> GenerateModuleStatisticsAsync(ReportParametersDto parameters, CancellationToken cancellationToken)
    {
        var totalModules = await _context.Modules.CountAsync(cancellationToken);
        var activeModules = await _context.Modules.CountAsync(m => m.IsActive, cancellationToken);

        var section = new ReportSectionDto
        {
            Title = "Module Statistics",
            Description = "Overview of available modules",
            Tables = new List<ReportTableDto>
            {
                new ReportTableDto
                {
                    Title = "Module Status",
                    Columns = new List<string> { "Status", "Count" },
                    Rows = new List<List<object>>
                    {
                        new List<object> { "Total Modules", totalModules },
                        new List<object> { "Active Modules", activeModules },
                        new List<object> { "Inactive Modules", totalModules - activeModules }
                    }
                }
            }
        };

        return section;
    }

    public Task<bool> ValidateParametersAsync(ReportParametersDto parameters)
    {
        // System overview doesn't have specific validation requirements
        return Task.FromResult(true);
    }

    public ReportParametersDto GetDefaultParameters()
    {
        return new ReportParametersDto
        {
            IncludeInactive = false,
            IncludeDetails = true
        };
    }
}

