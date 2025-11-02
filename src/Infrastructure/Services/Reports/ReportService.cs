using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Reports;

/// <summary>
/// Main report service orchestration layer
/// </summary>
public class ReportService : IReportService
{
    private readonly IApplicationDbContext _context;
    private readonly IReportGeneratorService _generatorService;
    private readonly IReportExportService _exportService;
    private readonly ISuperAdminContext _superAdminContext;
    private readonly ISuperAdminAuditService _auditService;
    private readonly ILogger<ReportService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _reportsStoragePath;
    private readonly int _downloadUrlExpirationHours;

    public ReportService(
        IApplicationDbContext context,
        IReportGeneratorService generatorService,
        IReportExportService exportService,
        ISuperAdminContext superAdminContext,
        ISuperAdminAuditService auditService,
        ILogger<ReportService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _generatorService = generatorService;
        _exportService = exportService;
        _superAdminContext = superAdminContext;
        _auditService = auditService;
        _logger = logger;
        _configuration = configuration;

        _reportsStoragePath = _configuration["Reports:StoragePath"] ?? "./Reports";
        _downloadUrlExpirationHours = int.Parse(_configuration["Reports:DownloadUrlExpiration"] ?? "24");

        // Ensure storage directory exists
        if (!Directory.Exists(_reportsStoragePath))
        {
            Directory.CreateDirectory(_reportsStoragePath);
        }
    }

    public async Task<Result<ReportResponseDto>> GenerateReportAsync(ReportRequestDto request, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var execution = new ReportExecution
        {
            Id = Guid.NewGuid(),
            StartedAt = startTime,
            Status = ReportStatus.Processing,
            InitiatedBy = _superAdminContext.SuperAdminId,
            IsScheduled = false
        };

        try
        {
            _logger.LogInformation("Starting report generation: {ReportName} ({ReportType})", request.Name, request.Type);

            // Create report entity
            var report = new Report
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Type = request.Type,
                Category = request.Category,
                Format = request.Format,
                Status = ReportStatus.Processing,
                GeneratedBy = _superAdminContext.SuperAdminId ?? Guid.Empty,
                GeneratedAt = startTime,
                TenantId = request.TenantId,
                Parameters = System.Text.Json.JsonSerializer.Serialize(request.Parameters),
                Filters = System.Text.Json.JsonSerializer.Serialize(request.Filters)
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync(cancellationToken);

            // Validate parameters
            var parameters = request.Parameters ?? _generatorService.GetDefaultParameters(request.Type);
            var isValid = await _generatorService.ValidateParametersAsync(request.Type, parameters);

            if (!isValid)
            {
                throw new InvalidOperationException("Invalid report parameters");
            }

            // Generate report data
            var reportData = await _generatorService.GenerateReportDataAsync(request.Type, parameters, cancellationToken);

            // Export to requested format
            var reportFile = await _exportService.ExportAsync(reportData, request.Format, request.Name, cancellationToken);

            // Save file to storage
            var fullPath = Path.Combine(_reportsStoragePath, reportFile.FileName);
            await File.WriteAllBytesAsync(fullPath, reportFile.FileContent, cancellationToken);

            // Generate download URL (in production, this would be a signed URL to blob storage)
            var downloadUrl = $"/cp/reports/{report.Id}/download";
            var expiresAt = DateTime.UtcNow.AddHours(_downloadUrlExpirationHours);

            // Update report with completion info
            report.Status = ReportStatus.Completed;
            report.CompletedAt = DateTime.UtcNow;
            report.FilePath = fullPath;
            report.FileSize = reportFile.FileSize;
            report.DownloadUrl = downloadUrl;
            report.ExpiresAt = expiresAt;
            report.RecordCount = reportData.Summary?.TotalRecords ?? 0;
            report.GenerationDurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            // Update execution
            execution.ReportId = report.Id;
            execution.Status = ReportStatus.Completed;
            execution.CompletedAt = DateTime.UtcNow;
            execution.DurationMs = report.GenerationDurationMs;
            execution.RecordCount = report.RecordCount;

            _context.ReportExecutions.Add(execution);
            await _context.SaveChangesAsync(cancellationToken);

            // Audit log
            await _auditService.LogActionAsync(
                "GenerateReport",
                "Report",
                report.Id.ToString(),
                report.TenantId,
                $"Generated report '{report.Name}' ({report.Type}) in {report.Format} format");

            _logger.LogInformation("Report generated successfully: {ReportId}, Duration: {Duration}ms",
                report.Id, report.GenerationDurationMs);

            return Result<ReportResponseDto>.Success(MapToDto(report), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report: {ReportName}", request.Name);

            // Update execution with error
            execution.Status = ReportStatus.Failed;
            execution.CompletedAt = DateTime.UtcNow;
            execution.DurationMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            execution.ErrorMessage = ex.Message;
            execution.StackTrace = ex.StackTrace;

            _context.ReportExecutions.Add(execution);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<ReportResponseDto>.Failure($"Failed to generate report: {ex.Message}", 500);
        }
    }

    public async Task<Result<ReportResponseDto>> GetReportByIdAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await _context.Reports.FindAsync(new object[] { reportId }, cancellationToken);

            if (report == null)
            {
                return Result<ReportResponseDto>.Failure("Report not found", 404);
            }

            return Result<ReportResponseDto>.Success(MapToDto(report), 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report: {ReportId}", reportId);
            return Result<ReportResponseDto>.Failure($"Error retrieving report: {ex.Message}", 500);
        }
    }

    public async Task<Result<PaginatedResult<ReportResponseDto>>> GetReportsAsync(ReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Reports.AsQueryable();

            // Apply filters
            if (filter.Type.HasValue)
            {
                query = query.Where(r => r.Type == filter.Type.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(r => r.Status == filter.Status.Value);
            }

            if (filter.Format.HasValue)
            {
                query = query.Where(r => r.Format == filter.Format.Value);
            }

            if (filter.GeneratedBy.HasValue)
            {
                query = query.Where(r => r.GeneratedBy == filter.GeneratedBy.Value);
            }

            if (filter.TenantId.HasValue)
            {
                query = query.Where(r => r.TenantId == filter.TenantId.Value);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(r => r.GeneratedAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(r => r.GeneratedAt <= filter.ToDate.Value);
            }

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(r => r.Name.Contains(filter.SearchTerm) ||
                                        (r.Description != null && r.Description.Contains(filter.SearchTerm)));
            }

            // Apply sorting
            query = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
                "type" => filter.SortDescending ? query.OrderByDescending(r => r.Type) : query.OrderBy(r => r.Type),
                "status" => filter.SortDescending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
                "generatedat" => filter.SortDescending ? query.OrderByDescending(r => r.GeneratedAt) : query.OrderBy(r => r.GeneratedAt),
                _ => query.OrderByDescending(r => r.GeneratedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var reports = await query
                .Skip((filter.Page - 1) * filter.Size)
                .Take(filter.Size)
                .ToListAsync(cancellationToken);

            var result = new PaginatedResult<ReportResponseDto>
            {
                Items = reports.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = filter.Page,
                Size = filter.Size
            };

            return Result<PaginatedResult<ReportResponseDto>>.Success(result, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reports");
            return Result<PaginatedResult<ReportResponseDto>>.Failure($"Error getting reports: {ex.Message}", 500);
        }
    }

    public async Task<Result<bool>> DeleteReportAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await _context.Reports.FindAsync(new object[] { reportId }, cancellationToken);

            if (report == null)
            {
                return Result<bool>.Failure("Report not found", 404);
            }

            // Delete physical file if exists
            if (!string.IsNullOrEmpty(report.FilePath) && File.Exists(report.FilePath))
            {
                File.Delete(report.FilePath);
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync(cancellationToken);

            await _auditService.LogActionAsync(
                "DeleteReport",
                "Report",
                reportId.ToString(),
                report.TenantId,
                $"Deleted report '{report.Name}'");

            _logger.LogInformation("Report deleted: {ReportId}", reportId);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting report: {ReportId}", reportId);
            return Result<bool>.Failure($"Error deleting report: {ex.Message}", 500);
        }
    }

    public async Task<Result<string>> GetDownloadUrlAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await _context.Reports.FindAsync(new object[] { reportId }, cancellationToken);

            if (report == null)
            {
                return Result<string>.Failure("Report not found", 404);
            }

            if (report.Status != ReportStatus.Completed)
            {
                return Result<string>.Failure("Report is not ready for download", 400);
            }

            if (report.ExpiresAt.HasValue && report.ExpiresAt.Value < DateTime.UtcNow)
            {
                report.Status = ReportStatus.Expired;
                await _context.SaveChangesAsync(cancellationToken);
                return Result<string>.Failure("Report download link has expired", 410);
            }

            return Result<string>.Success(report.DownloadUrl ?? "", 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting download URL for report: {ReportId}", reportId);
            return Result<string>.Failure($"Error getting download URL: {ex.Message}", 500);
        }
    }

    public async Task<Result<PaginatedResult<ReportExecutionDto>>> GetReportHistoryAsync(ReportHistoryFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.ReportExecutions.AsQueryable();

            if (filter.ReportScheduleId.HasValue)
            {
                query = query.Where(e => e.ReportScheduleId == filter.ReportScheduleId.Value);
            }

            if (filter.InitiatedBy.HasValue)
            {
                query = query.Where(e => e.InitiatedBy == filter.InitiatedBy.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(e => e.Status == filter.Status.Value);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(e => e.StartedAt >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(e => e.StartedAt <= filter.ToDate.Value);
            }

            if (filter.ScheduledOnly)
            {
                query = query.Where(e => e.IsScheduled);
            }

            query = filter.SortDescending
                ? query.OrderByDescending(e => e.StartedAt)
                : query.OrderBy(e => e.StartedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var executions = await query
                .Skip((filter.Page - 1) * filter.Size)
                .Take(filter.Size)
                .ToListAsync(cancellationToken);

            var result = new PaginatedResult<ReportExecutionDto>
            {
                Items = executions.Select(MapExecutionToDto).ToList(),
                TotalCount = totalCount,
                Page = filter.Page,
                Size = filter.Size
            };

            return Result<PaginatedResult<ReportExecutionDto>>.Success(result, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report history");
            return Result<PaginatedResult<ReportExecutionDto>>.Failure($"Error getting report history: {ex.Message}", 500);
        }
    }

    public async Task<Result<ReportAnalyticsDto>> GetReportAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var totalReports = await _context.Reports.CountAsync(cancellationToken);
            var completedReports = await _context.Reports.CountAsync(r => r.Status == ReportStatus.Completed, cancellationToken);
            var failedReports = await _context.Reports.CountAsync(r => r.Status == ReportStatus.Failed, cancellationToken);
            var pendingReports = await _context.Reports.CountAsync(r => r.Status == ReportStatus.Pending || r.Status == ReportStatus.Processing, cancellationToken);

            var reportsByType = await _context.Reports
                .GroupBy(r => r.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);

            var reportsByFormat = await _context.Reports
                .GroupBy(r => r.Format)
                .Select(g => new { Format = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Format, x => x.Count, cancellationToken);

            var averageTime = await _context.Reports
                .Where(r => r.GenerationDurationMs.HasValue)
                .AverageAsync(r => (long?)r.GenerationDurationMs, cancellationToken) ?? 0;

            var totalFileSize = await _context.Reports
                .Where(r => r.FileSize.HasValue)
                .SumAsync(r => (long?)r.FileSize, cancellationToken) ?? 0;

            var activeSchedules = await _context.ReportSchedules.CountAsync(s => s.IsActive, cancellationToken);
            var totalSubscriptions = await _context.ReportSubscriptions.CountAsync(s => s.IsActive, cancellationToken);

            var analytics = new ReportAnalyticsDto
            {
                TotalReports = totalReports,
                CompletedReports = completedReports,
                FailedReports = failedReports,
                PendingReports = pendingReports,
                ReportsByType = reportsByType,
                ReportsByFormat = reportsByFormat,
                AverageGenerationTimeMs = (long)averageTime,
                TotalFileSize = totalFileSize,
                ActiveSchedules = activeSchedules,
                TotalSubscriptions = totalSubscriptions
            };

            return Result<ReportAnalyticsDto>.Success(analytics, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting report analytics");
            return Result<ReportAnalyticsDto>.Failure($"Error getting report analytics: {ex.Message}", 500);
        }
    }

    public async Task<Result<List<ReportTypeDto>>> GetReportTypesAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        var reportTypes = new List<ReportTypeDto>
        {
            new ReportTypeDto
            {
                Type = ReportType.SystemOverview,
                Name = "System Overview",
                Description = "Comprehensive overview of the system status including tenants, licenses, plans, and modules",
                Category = ReportCategory.Operations,
                SupportedFormats = _exportService.GetSupportedFormats(),
                RequiresTenantContext = false
            },
            new ReportTypeDto
            {
                Type = ReportType.Financial,
                Name = "Financial Report",
                Description = "Financial analytics including revenue by plan, license sales, and growth trends",
                Category = ReportCategory.Financial,
                SupportedFormats = _exportService.GetSupportedFormats(),
                RequiresTenantContext = false
            },
            new ReportTypeDto
            {
                Type = ReportType.TenantUsage,
                Name = "Tenant Usage",
                Description = "Resource utilization and usage metrics by tenant",
                Category = ReportCategory.Operations,
                SupportedFormats = _exportService.GetSupportedFormats(),
                RequiresTenantContext = false
            },
            new ReportTypeDto
            {
                Type = ReportType.Audit,
                Name = "Audit Report",
                Description = "Comprehensive audit logs for SuperAdmin and tenant activities",
                Category = ReportCategory.Compliance,
                SupportedFormats = _exportService.GetSupportedFormats(),
                RequiresTenantContext = false
            },
            new ReportTypeDto
            {
                Type = ReportType.Analytics,
                Name = "Analytics Report",
                Description = "System analytics including user growth trends, module adoption, and license utilization",
                Category = ReportCategory.Performance,
                SupportedFormats = _exportService.GetSupportedFormats(),
                RequiresTenantContext = false
            }
        };

        return Result<List<ReportTypeDto>>.Success(reportTypes, 200);
    }

    public async Task<Result<int>> CleanupExpiredReportsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var expiredReports = await _context.Reports
                .Where(r => r.ExpiresAt.HasValue && r.ExpiresAt.Value < DateTime.UtcNow && r.Status == ReportStatus.Completed)
                .ToListAsync(cancellationToken);

            int deletedCount = 0;

            foreach (var report in expiredReports)
            {
                // Delete physical file
                if (!string.IsNullOrEmpty(report.FilePath) && File.Exists(report.FilePath))
                {
                    File.Delete(report.FilePath);
                }

                report.Status = ReportStatus.Expired;
                deletedCount++;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cleaned up {Count} expired reports", deletedCount);

            return Result<int>.Success(deletedCount, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired reports");
            return Result<int>.Failure($"Error cleaning up expired reports: {ex.Message}", 500);
        }
    }

    private ReportResponseDto MapToDto(Report report)
    {
        return new ReportResponseDto
        {
            Id = report.Id,
            Name = report.Name,
            Description = report.Description,
            Type = report.Type,
            Category = report.Category,
            Format = report.Format,
            Status = report.Status,
            GeneratedBy = report.GeneratedBy,
            GeneratedAt = report.GeneratedAt,
            CompletedAt = report.CompletedAt,
            DownloadUrl = report.DownloadUrl,
            ExpiresAt = report.ExpiresAt,
            FileSize = report.FileSize,
            RecordCount = report.RecordCount,
            ErrorMessage = report.ErrorMessage,
            GenerationDurationMs = report.GenerationDurationMs,
            TenantId = report.TenantId,
            ReportScheduleId = report.ReportScheduleId
        };
    }

    private ReportExecutionDto MapExecutionToDto(ReportExecution execution)
    {
        return new ReportExecutionDto
        {
            Id = execution.Id,
            ReportScheduleId = execution.ReportScheduleId,
            ReportId = execution.ReportId,
            Status = execution.Status,
            StartedAt = execution.StartedAt,
            CompletedAt = execution.CompletedAt,
            DurationMs = execution.DurationMs,
            RecordCount = execution.RecordCount,
            ErrorMessage = execution.ErrorMessage,
            InitiatedBy = execution.InitiatedBy,
            IsScheduled = execution.IsScheduled
        };
    }
}

