using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Services.Reports.Exporters;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Reports;

/// <summary>
/// Service for exporting reports to different formats
/// </summary>
public class ReportExportService : IReportExportService
{
    private readonly Dictionary<ReportFormat, IReportExporter> _exporters;
    private readonly ILogger<ReportExportService> _logger;

    public ReportExportService(
        PdfReportExporter pdfExporter,
        ExcelReportExporter excelExporter,
        CsvReportExporter csvExporter,
        JsonReportExporter jsonExporter,
        HtmlReportExporter htmlExporter,
        ILogger<ReportExportService> logger)
    {
        _logger = logger;

        // Register all exporters
        _exporters = new Dictionary<ReportFormat, IReportExporter>
        {
            { ReportFormat.PDF, pdfExporter },
            { ReportFormat.Excel, excelExporter },
            { ReportFormat.CSV, csvExporter },
            { ReportFormat.JSON, jsonExporter },
            { ReportFormat.HTML, htmlExporter }
        };
    }

    public async Task<ReportFileDto> ExportAsync(ReportDataDto reportData, ReportFormat format, string reportName, CancellationToken cancellationToken = default)
    {
        if (!_exporters.TryGetValue(format, out var exporter))
        {
            _logger.LogError("No exporter found for format: {Format}", format);
            throw new NotSupportedException($"Export format {format} is not supported");
        }

        _logger.LogInformation("Exporting report '{ReportName}' to format: {Format}", reportName, format);

        try
        {
            var fileContent = await exporter.ExportAsync(reportData, reportName, cancellationToken);
            var fileName = $"{SanitizeFileName(reportName)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{exporter.GetFileExtension()}";
            var filePath = Path.Combine("Reports", fileName);

            var reportFile = new ReportFileDto
            {
                FileName = fileName,
                FilePath = filePath,
                FileContent = fileContent,
                MimeType = exporter.GetMimeType(),
                FileSize = fileContent.Length
            };

            _logger.LogInformation("Successfully exported report '{ReportName}' to {Format}, Size: {Size} bytes",
                reportName, format, fileContent.Length);

            return reportFile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report '{ReportName}' to format: {Format}", reportName, format);
            throw;
        }
    }

    public List<ReportFormat> GetSupportedFormats()
    {
        return _exporters.Keys.ToList();
    }

    public bool IsFormatSupported(ReportFormat format)
    {
        return _exporters.ContainsKey(format);
    }

    public string GetMimeType(ReportFormat format)
    {
        if (!_exporters.TryGetValue(format, out var exporter))
        {
            throw new NotSupportedException($"Export format {format} is not supported");
        }

        return exporter.GetMimeType();
    }

    public string GetFileExtension(ReportFormat format)
    {
        if (!_exporters.TryGetValue(format, out var exporter))
        {
            throw new NotSupportedException($"Export format {format} is not supported");
        }

        return exporter.GetFileExtension();
    }

    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        return sanitized.Replace(" ", "_");
    }
}

