using HotelManagement.Application.Common.DTOs;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Reports.Exporters;

/// <summary>
/// CSV report exporter using CsvHelper
/// </summary>
public class CsvReportExporter : IReportExporter
{
    private readonly ILogger<CsvReportExporter> _logger;

    public CsvReportExporter(ILogger<CsvReportExporter> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> ExportAsync(ReportDataDto reportData, string reportName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting report to CSV: {ReportName}", reportName);

        return await Task.Run(() =>
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csv = new CsvWriter(streamWriter, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            });

            // Write report header
            csv.WriteField("Report Title");
            csv.WriteField(reportData.Title);
            csv.NextRecord();

            csv.WriteField("Description");
            csv.WriteField(reportData.Description ?? "");
            csv.NextRecord();

            csv.WriteField("Generated At");
            csv.WriteField(reportData.GeneratedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            csv.NextRecord();

            csv.WriteField("Report Type");
            csv.WriteField(reportData.Type.ToString());
            csv.NextRecord();

            csv.NextRecord(); // Empty line

            // Write each section
            foreach (var section in reportData.Sections)
            {
                WriteSectionToCsv(csv, section);
                csv.NextRecord(); // Empty line between sections
            }

            // Write summary if available
            if (reportData.Summary != null)
            {
                WriteSummaryToCsv(csv, reportData.Summary);
            }

            streamWriter.Flush();
            return memoryStream.ToArray();
        }, cancellationToken);
    }

    private void WriteSectionToCsv(CsvWriter csv, ReportSectionDto section)
    {
        // Section header
        csv.WriteField("Section");
        csv.WriteField(section.Title);
        csv.NextRecord();

        if (!string.IsNullOrEmpty(section.Description))
        {
            csv.WriteField("Description");
            csv.WriteField(section.Description);
            csv.NextRecord();
        }

        csv.NextRecord();

        // Write each table in the section
        foreach (var table in section.Tables)
        {
            WriteTableToCsv(csv, table);
            csv.NextRecord(); // Empty line between tables
        }
    }

    private void WriteTableToCsv(CsvWriter csv, ReportTableDto table)
    {
        if (!string.IsNullOrEmpty(table.Title))
        {
            csv.WriteField("Table");
            csv.WriteField(table.Title);
            csv.NextRecord();
        }

        // Write column headers
        foreach (var column in table.Columns)
        {
            csv.WriteField(column);
        }
        csv.NextRecord();

        // Write data rows
        foreach (var row in table.Rows)
        {
            foreach (var cell in row)
            {
                csv.WriteField(cell?.ToString() ?? "");
            }
            csv.NextRecord();
        }
    }

    private void WriteSummaryToCsv(CsvWriter csv, ReportSummaryDto summary)
    {
        csv.WriteField("Summary");
        csv.NextRecord();

        csv.WriteField("Total Records");
        csv.WriteField(summary.TotalRecords);
        csv.NextRecord();

        csv.NextRecord();

        csv.WriteField("Statistics");
        csv.NextRecord();

        csv.WriteField("Metric");
        csv.WriteField("Value");
        csv.NextRecord();

        foreach (var stat in summary.Statistics)
        {
            csv.WriteField(stat.Key);
            csv.WriteField(stat.Value?.ToString() ?? "");
            csv.NextRecord();
        }
    }

    public string GetMimeType() => "text/csv";

    public string GetFileExtension() => ".csv";
}

