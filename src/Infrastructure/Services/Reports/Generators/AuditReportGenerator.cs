using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Reports.Generators;

/// <summary>
/// Generator for audit reports
/// </summary>
public class AuditReportGenerator : IReportDataGenerator
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AuditReportGenerator> _logger;

    public ReportType ReportType => ReportType.Audit;

    public AuditReportGenerator(
        IApplicationDbContext context,
        ILogger<AuditReportGenerator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ReportDataDto> GenerateAsync(ReportParametersDto parameters, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Audit Report");

        var startDate = parameters.StartDate ?? DateTime.UtcNow.AddDays(-7);
        var endDate = parameters.EndDate ?? DateTime.UtcNow;

        // Generate sections
        var superAdminAuditSection = await GenerateSuperAdminAuditSectionAsync(startDate, endDate, cancellationToken);
        var tenantAuditSection = await GenerateTenantAuditSectionAsync(startDate, endDate, parameters.TenantIds, cancellationToken);

        // Calculate summary
        var totalAuditLogs = await _context.SuperAdminAuditLogs
            .CountAsync(a => a.Timestamp >= startDate && a.Timestamp <= endDate, cancellationToken);

        var reportData = new ReportDataDto
        {
            Title = "Audit Report",
            Description = $"Audit log report from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
            Type = ReportType.Audit,
            GeneratedAt = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                { "StartDate", startDate },
                { "EndDate", endDate }
            },
            Sections = new List<ReportSectionDto>
            {
                superAdminAuditSection,
                tenantAuditSection
            },
            Summary = new ReportSummaryDto
            {
                TotalRecords = totalAuditLogs,
                Statistics = new Dictionary<string, object>
                {
                    { "TotalAuditLogs", totalAuditLogs },
                    { "Period", $"{(endDate - startDate).Days} days" }
                }
            }
        };

        return reportData;
    }

    private async Task<ReportSectionDto> GenerateSuperAdminAuditSectionAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var auditLogs = await _context.SuperAdminAuditLogs
            .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
            .OrderByDescending(a => a.Timestamp)
            .Take(100)
            .Select(a => new
            {
                a.Timestamp,
                a.Username,
                a.Action,
                a.TargetType,
                a.TargetId
            })
            .ToListAsync(cancellationToken);

        var section = new ReportSectionDto
        {
            Title = "SuperAdmin Audit Logs",
            Description = "Recent SuperAdmin activities",
            Tables = new List<ReportTableDto>
            {
                new ReportTableDto
                {
                    Title = "Recent Activities (Top 100)",
                    Columns = new List<string> { "Timestamp", "Username", "Action", "Target Type", "Target ID" },
                    Rows = auditLogs.Select(a => new List<object> 
                    { 
                        a.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"), 
                        a.Username, 
                        a.Action, 
                        a.TargetType, 
                        a.TargetId ?? "N/A" 
                    }).ToList()
                }
            }
        };

        return section;
    }

    private async Task<ReportSectionDto> GenerateTenantAuditSectionAsync(DateTime startDate, DateTime endDate, List<Guid>? tenantIds, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate);

        if (tenantIds != null && tenantIds.Any())
        {
            query = query.Where(a => a.User.TenantId.HasValue && tenantIds.Contains(a.User.TenantId.Value));
        }

        var auditLogsByAction = await query
            .GroupBy(a => a.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var section = new ReportSectionDto
        {
            Title = "Tenant Audit Logs Summary",
            Description = "Tenant activities breakdown",
            Tables = new List<ReportTableDto>
            {
                new ReportTableDto
                {
                    Title = "Actions by Type",
                    Columns = new List<string> { "Action", "Count" },
                    Rows = auditLogsByAction.Select(a => new List<object> { a.Action, a.Count }).ToList()
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
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };
    }
}

