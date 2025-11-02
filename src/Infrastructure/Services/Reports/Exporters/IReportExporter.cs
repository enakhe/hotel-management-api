using HotelManagement.Application.Common.DTOs;

namespace HotelManagement.Infrastructure.Services.Reports.Exporters;

/// <summary>
/// Interface for report exporters
/// </summary>
public interface IReportExporter
{
    /// <summary>
    /// Export report data to byte array
    /// </summary>
    Task<byte[]> ExportAsync(ReportDataDto reportData, string reportName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get MIME type for this export format
    /// </summary>
    string GetMimeType();

    /// <summary>
    /// Get file extension for this export format
    /// </summary>
    string GetFileExtension();
}

