using HotelManagement.Application.Common.DTOs;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Infrastructure.Services.Reports.Generators;

/// <summary>
/// Interface for report data generators
/// </summary>
public interface IReportDataGenerator
{
    /// <summary>
    /// The report type this generator handles
    /// </summary>
    ReportType ReportType { get; }

    /// <summary>
    /// Generate report data
    /// </summary>
    Task<ReportDataDto> GenerateAsync(ReportParametersDto parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate parameters for this report type
    /// </summary>
    Task<bool> ValidateParametersAsync(ReportParametersDto parameters);

    /// <summary>
    /// Get default parameters for this report type
    /// </summary>
    ReportParametersDto GetDefaultParameters();
}

