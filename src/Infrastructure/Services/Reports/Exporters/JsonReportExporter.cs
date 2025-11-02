using HotelManagement.Application.Common.DTOs;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Reports.Exporters;

/// <summary>
/// JSON report exporter
/// </summary>
public class JsonReportExporter : IReportExporter
{
    private readonly ILogger<JsonReportExporter> _logger;

    public JsonReportExporter(ILogger<JsonReportExporter> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> ExportAsync(ReportDataDto reportData, string reportName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting report to JSON: {ReportName}", reportName);

        return await Task.Run(() =>
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(reportData, options);
            return Encoding.UTF8.GetBytes(json);
        }, cancellationToken);
    }

    public string GetMimeType() => "application/json";

    public string GetFileExtension() => ".json";
}

