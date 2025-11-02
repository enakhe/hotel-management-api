using HotelManagement.Application.Common.DTOs;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.Interfaces;

/// <summary>
/// Service for exporting reports to different formats
/// </summary>
public interface IReportExportService
{
    /// <summary>
    /// Export report data to specified format
    /// </summary>
    Task<ReportFileDto> ExportAsync(ReportDataDto reportData, ReportFormat format, string reportName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get supported export formats
    /// </summary>
    List<ReportFormat> GetSupportedFormats();

    /// <summary>
    /// Validate if format is supported
    /// </summary>
    bool IsFormatSupported(ReportFormat format);

    /// <summary>
    /// Get MIME type for format
    /// </summary>
    string GetMimeType(ReportFormat format);

    /// <summary>
    /// Get file extension for format
    /// </summary>
    string GetFileExtension(ReportFormat format);
}

