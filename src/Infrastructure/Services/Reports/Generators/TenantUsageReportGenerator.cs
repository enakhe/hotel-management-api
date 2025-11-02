using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Reports.Generators;

/// <summary>
/// Generator for tenant usage reports
/// </summary>
public class TenantUsageReportGenerator : IReportDataGenerator
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TenantUsageReportGenerator> _logger;

    public ReportType ReportType => ReportType.TenantUsage;

    public TenantUsageReportGenerator(
        IApplicationDbContext context,
        ILogger<TenantUsageReportGenerator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ReportDataDto> GenerateAsync(ReportParametersDto parameters, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Tenant Usage Report");

        var tenantIds = parameters.TenantIds ?? new List<Guid>();
        var query = _context.Tenants.AsQueryable();

        if (tenantIds.Any())
        {
            query = query.Where(t => tenantIds.Contains(t.Id));
        }

        if (!parameters.IncludeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        var tenants = await query
            .Include(t => t.Branches)
            .Include(t => t.Plan)
            .ToListAsync(cancellationToken);

        // Tenant Resource Usage Section
        var rows = new List<List<object>>();
        foreach (var tenant in tenants)
        {
            var userCount = await _context.Users.CountAsync(u => u.TenantId == tenant.Id, cancellationToken);
            var roomCount = await _context.Rooms.CountAsync(r => r.TenantId == tenant.Id, cancellationToken);
            var reservationCount = await _context.Reservations.CountAsync(r => r.TenantId == tenant.Id, cancellationToken);

            rows.Add(new List<object>
            {
                tenant.Name,
                tenant.Plan.Name,
                tenant.Branches.Count,
                userCount,
                roomCount,
                reservationCount,
                tenant.IsActive ? "Active" : "Inactive"
            });
        }

        var section = new ReportSectionDto
        {
            Title = "Tenant Resource Usage",
            Description = "Detailed usage metrics for each tenant",
            Tables = new List<ReportTableDto>
            {
                new ReportTableDto
                {
                    Title = "Usage Summary",
                    Columns = new List<string> { "Tenant", "Plan", "Branches", "Users", "Rooms", "Reservations", "Status" },
                    Rows = rows
                }
            }
        };

        var reportData = new ReportDataDto
        {
            Title = "Tenant Usage Report",
            Description = "Resource utilization and usage metrics by tenant",
            Type = ReportType.TenantUsage,
            GeneratedAt = DateTime.UtcNow,
            Sections = new List<ReportSectionDto> { section },
            Summary = new ReportSummaryDto
            {
                TotalRecords = tenants.Count,
                Statistics = new Dictionary<string, object>
                {
                    { "TotalTenants", tenants.Count },
                    { "TotalBranches", tenants.Sum(t => t.Branches.Count) },
                    { "AverageBranchesPerTenant", tenants.Any() ? tenants.Average(t => t.Branches.Count) : 0 }
                }
            }
        };

        return reportData;
    }

    public Task<bool> ValidateParametersAsync(ReportParametersDto parameters)
    {
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

