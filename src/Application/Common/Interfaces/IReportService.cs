using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Main report service interface for report management operations
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Generate a report based on the request
    /// </summary>
    Task<Result<ReportResponseDto>> GenerateReportAsync(ReportRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get report by ID
    /// </summary>
    Task<Result<ReportResponseDto>> GetReportByIdAsync(Guid reportId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get reports with pagination and filtering
    /// </summary>
    Task<Result<PaginatedResult<ReportResponseDto>>> GetReportsAsync(ReportFilterDto filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a report
    /// </summary>
    Task<Result<bool>> DeleteReportAsync(Guid reportId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get download URL for a report
    /// </summary>
    Task<Result<string>> GetDownloadUrlAsync(Guid reportId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get report generation history
    /// </summary>
    Task<Result<PaginatedResult<ReportExecutionDto>>> GetReportHistoryAsync(ReportHistoryFilterDto filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get report analytics
    /// </summary>
    Task<Result<ReportAnalyticsDto>> GetReportAnalyticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available report types
    /// </summary>
    Task<Result<List<ReportTypeDto>>> GetReportTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleanup expired reports
    /// </summary>
    Task<Result<int>> CleanupExpiredReportsAsync(CancellationToken cancellationToken = default);
}

