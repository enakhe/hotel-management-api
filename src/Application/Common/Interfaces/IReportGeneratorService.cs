using HotelManagement.Application.Common.DTOs;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for generating report data
/// </summary>
public interface IReportGeneratorService
{
    /// <summary>
    /// Generate report data based on type and parameters
    /// </summary>
    Task<ReportDataDto> GenerateReportDataAsync(ReportType reportType, ReportParametersDto parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate report parameters
    /// </summary>
    Task<bool> ValidateParametersAsync(ReportType reportType, ReportParametersDto parameters);

    /// <summary>
    /// Get default parameters for a report type
    /// </summary>
    ReportParametersDto GetDefaultParameters(ReportType reportType);
}

