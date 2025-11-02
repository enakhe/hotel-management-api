using HotelManagement.Application.Common.DTOs;
using System.Text;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Reports.Exporters;

/// <summary>
/// HTML report exporter
/// </summary>
public class HtmlReportExporter : IReportExporter
{
    private readonly ILogger<HtmlReportExporter> _logger;

    public HtmlReportExporter(ILogger<HtmlReportExporter> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> ExportAsync(ReportDataDto reportData, string reportName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting report to HTML: {ReportName}", reportName);

        return await Task.Run(() =>
        {
            var html = GenerateHtml(reportData);
            return Encoding.UTF8.GetBytes(html);
        }, cancellationToken);
    }

    private string GenerateHtml(ReportDataDto reportData)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine($"    <title>{reportData.Title}</title>");
        sb.AppendLine("    <style>");
        sb.AppendLine(GetStyles());
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        
        // Header
        sb.AppendLine("    <div class=\"header\">");
        sb.AppendLine($"        <h1>{reportData.Title}</h1>");
        
        if (!string.IsNullOrEmpty(reportData.Description))
        {
            sb.AppendLine($"        <p class=\"description\">{reportData.Description}</p>");
        }
        
        sb.AppendLine($"        <p class=\"meta\">Generated: {reportData.GeneratedAt:yyyy-MM-dd HH:mm:ss}</p>");
        sb.AppendLine($"        <p class=\"meta\">Report Type: {reportData.Type}</p>");
        sb.AppendLine("    </div>");

        // Metadata
        if (reportData.Metadata != null && reportData.Metadata.Any())
        {
            sb.AppendLine("    <div class=\"metadata\">");
            sb.AppendLine("        <h3>Metadata</h3>");
            sb.AppendLine("        <table class=\"metadata-table\">");
            foreach (var meta in reportData.Metadata)
            {
                sb.AppendLine("            <tr>");
                sb.AppendLine($"                <td><strong>{meta.Key}:</strong></td>");
                sb.AppendLine($"                <td>{meta.Value}</td>");
                sb.AppendLine("            </tr>");
            }
            sb.AppendLine("        </table>");
            sb.AppendLine("    </div>");
        }

        // Sections
        foreach (var section in reportData.Sections)
        {
            sb.AppendLine(GenerateSectionHtml(section));
        }

        // Summary
        if (reportData.Summary != null)
        {
            sb.AppendLine(GenerateSummaryHtml(reportData.Summary));
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private string GenerateSectionHtml(ReportSectionDto section)
    {
        var sb = new StringBuilder();

        sb.AppendLine("    <div class=\"section\">");
        sb.AppendLine($"        <h2>{section.Title}</h2>");
        
        if (!string.IsNullOrEmpty(section.Description))
        {
            sb.AppendLine($"        <p class=\"section-description\">{section.Description}</p>");
        }

        // Tables
        foreach (var table in section.Tables)
        {
            sb.AppendLine(GenerateTableHtml(table));
        }

        sb.AppendLine("    </div>");

        return sb.ToString();
    }

    private string GenerateTableHtml(ReportTableDto table)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(table.Title))
        {
            sb.AppendLine($"        <h3 class=\"table-title\">{table.Title}</h3>");
        }

        sb.AppendLine("        <table class=\"data-table\">");
        sb.AppendLine("            <thead>");
        sb.AppendLine("                <tr>");
        
        foreach (var column in table.Columns)
        {
            sb.AppendLine($"                    <th>{column}</th>");
        }
        
        sb.AppendLine("                </tr>");
        sb.AppendLine("            </thead>");
        sb.AppendLine("            <tbody>");

        foreach (var row in table.Rows)
        {
            sb.AppendLine("                <tr>");
            foreach (var cell in row)
            {
                sb.AppendLine($"                    <td>{cell?.ToString() ?? ""}</td>");
            }
            sb.AppendLine("                </tr>");
        }

        sb.AppendLine("            </tbody>");
        sb.AppendLine("        </table>");

        return sb.ToString();
    }

    private string GenerateSummaryHtml(ReportSummaryDto summary)
    {
        var sb = new StringBuilder();

        sb.AppendLine("    <div class=\"summary\">");
        sb.AppendLine("        <h2>Summary</h2>");
        sb.AppendLine($"        <p><strong>Total Records:</strong> {summary.TotalRecords}</p>");

        if (summary.Statistics.Any())
        {
            sb.AppendLine("        <h3>Statistics</h3>");
            sb.AppendLine("        <table class=\"summary-table\">");
            
            foreach (var stat in summary.Statistics)
            {
                sb.AppendLine("            <tr>");
                sb.AppendLine($"                <td><strong>{stat.Key}:</strong></td>");
                sb.AppendLine($"                <td>{stat.Value}</td>");
                sb.AppendLine("            </tr>");
            }
            
            sb.AppendLine("        </table>");
        }

        sb.AppendLine("    </div>");

        return sb.ToString();
    }

    private string GetStyles()
    {
        return @"
        body {
            font-family: Arial, sans-serif;
            margin: 20px;
            background-color: #f5f5f5;
        }
        .header {
            background-color: white;
            padding: 20px;
            border-radius: 5px;
            margin-bottom: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        h1 {
            color: #1976d2;
            margin: 0 0 10px 0;
        }
        .description {
            color: #666;
            margin: 5px 0;
        }
        .meta {
            color: #999;
            font-size: 14px;
            margin: 2px 0;
        }
        .metadata {
            background-color: white;
            padding: 15px;
            border-radius: 5px;
            margin-bottom: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        .metadata-table {
            width: 100%;
        }
        .metadata-table td {
            padding: 5px;
        }
        .section {
            background-color: white;
            padding: 20px;
            border-radius: 5px;
            margin-bottom: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        h2 {
            color: #1976d2;
            border-bottom: 2px solid #1976d2;
            padding-bottom: 10px;
        }
        h3 {
            color: #333;
            margin-top: 15px;
        }
        .section-description {
            color: #666;
            font-style: italic;
            margin-bottom: 15px;
        }
        .table-title {
            color: #333;
            margin-bottom: 10px;
        }
        .data-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }
        .data-table thead {
            background-color: #1976d2;
            color: white;
        }
        .data-table th,
        .data-table td {
            padding: 12px;
            text-align: left;
            border: 1px solid #ddd;
        }
        .data-table tbody tr:nth-child(even) {
            background-color: #f9f9f9;
        }
        .data-table tbody tr:hover {
            background-color: #e3f2fd;
        }
        .summary {
            background-color: white;
            padding: 20px;
            border-radius: 5px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        .summary-table {
            width: 100%;
            margin-top: 10px;
        }
        .summary-table td {
            padding: 8px;
            border-bottom: 1px solid #eee;
        }
        @media print {
            body {
                background-color: white;
            }
            .section, .header, .metadata, .summary {
                box-shadow: none;
                page-break-inside: avoid;
            }
        }";
    }

    public string GetMimeType() => "text/html";

    public string GetFileExtension() => ".html";
}

