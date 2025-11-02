using HotelManagement.Application.Common.DTOs;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.Extensions.Logging;
using System.Drawing;

namespace HotelManagement.Infrastructure.Services.Reports.Exporters;

/// <summary>
/// Excel report exporter using EPPlus
/// </summary>
public class ExcelReportExporter : IReportExporter
{
    private readonly ILogger<ExcelReportExporter> _logger;

    public ExcelReportExporter(ILogger<ExcelReportExporter> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> ExportAsync(ReportDataDto reportData, string reportName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting report to Excel: {ReportName}", reportName);

        return await Task.Run(() =>
        {
            // Create Excel package with non-commercial license context
            using var package = new ExcelPackage();

            // Create overview sheet
            var overviewSheet = package.Workbook.Worksheets.Add("Overview");
            CreateOverviewSheet(overviewSheet, reportData);

            // Create a sheet for each section
            foreach (var section in reportData.Sections)
            {
                var sectionSheet = package.Workbook.Worksheets.Add(SanitizeSheetName(section.Title));
                CreateSectionSheet(sectionSheet, section);
            }

            // Create summary sheet if available
            if (reportData.Summary != null)
            {
                var summarySheet = package.Workbook.Worksheets.Add("Summary");
                CreateSummarySheet(summarySheet, reportData.Summary);
            }

            return package.GetAsByteArray();
        }, cancellationToken);
    }

    private void CreateOverviewSheet(ExcelWorksheet worksheet, ReportDataDto reportData)
    {
        worksheet.Cells["A1"].Value = reportData.Title;
        worksheet.Cells["A1"].Style.Font.Size = 16;
        worksheet.Cells["A1"].Style.Font.Bold = true;

        worksheet.Cells["A2"].Value = reportData.Description;
        worksheet.Cells["A3"].Value = $"Generated: {reportData.GeneratedAt:yyyy-MM-dd HH:mm:ss}";
        worksheet.Cells["A4"].Value = $"Report Type: {reportData.Type}";

        if (reportData.Metadata != null && reportData.Metadata.Any())
        {
            int row = 6;
            worksheet.Cells[$"A{row}"].Value = "Metadata:";
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            row++;

            foreach (var meta in reportData.Metadata)
            {
                worksheet.Cells[$"A{row}"].Value = meta.Key;
                worksheet.Cells[$"B{row}"].Value = meta.Value?.ToString();
                row++;
            }
        }

        worksheet.Cells.AutoFitColumns();
    }

    private void CreateSectionSheet(ExcelWorksheet worksheet, ReportSectionDto section)
    {
        int currentRow = 1;

        // Section header
        worksheet.Cells[$"A{currentRow}"].Value = section.Title;
        worksheet.Cells[$"A{currentRow}"].Style.Font.Size = 14;
        worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
        currentRow++;

        if (!string.IsNullOrEmpty(section.Description))
        {
            worksheet.Cells[$"A{currentRow}"].Value = section.Description;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Italic = true;
            currentRow++;
        }

        currentRow++;

        // Tables
        foreach (var table in section.Tables)
        {
            currentRow = CreateTable(worksheet, table, currentRow);
            currentRow += 2; // Add spacing between tables
        }

        worksheet.Cells.AutoFitColumns();
    }

    private int CreateTable(ExcelWorksheet worksheet, ReportTableDto table, int startRow)
    {
        int currentRow = startRow;

        // Table title
        if (!string.IsNullOrEmpty(table.Title))
        {
            worksheet.Cells[$"A{currentRow}"].Value = table.Title;
            worksheet.Cells[$"A{currentRow}"].Style.Font.Bold = true;
            currentRow++;
        }

        // Header row
        for (int col = 0; col < table.Columns.Count; col++)
        {
            var cell = worksheet.Cells[currentRow, col + 1];
            cell.Value = table.Columns[col];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }
        currentRow++;

        // Data rows
        foreach (var row in table.Rows)
        {
            for (int col = 0; col < row.Count; col++)
            {
                var cell = worksheet.Cells[currentRow, col + 1];
                cell.Value = row[col];
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            currentRow++;
        }

        return currentRow;
    }

    private void CreateSummarySheet(ExcelWorksheet worksheet, ReportSummaryDto summary)
    {
        worksheet.Cells["A1"].Value = "Report Summary";
        worksheet.Cells["A1"].Style.Font.Size = 14;
        worksheet.Cells["A1"].Style.Font.Bold = true;

        worksheet.Cells["A3"].Value = "Total Records:";
        worksheet.Cells["B3"].Value = summary.TotalRecords;
        worksheet.Cells["A3"].Style.Font.Bold = true;

        int row = 5;
        if (summary.Statistics.Any())
        {
            worksheet.Cells[$"A{row}"].Value = "Statistics:";
            worksheet.Cells[$"A{row}"].Style.Font.Bold = true;
            row++;

            foreach (var stat in summary.Statistics)
            {
                worksheet.Cells[$"A{row}"].Value = stat.Key;
                worksheet.Cells[$"B{row}"].Value = stat.Value?.ToString();
                row++;
            }
        }

        worksheet.Cells.AutoFitColumns();
    }

    private string SanitizeSheetName(string name)
    {
        // Excel sheet names have restrictions
        var sanitized = new string(name.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray());
        return sanitized.Length > 31 ? sanitized.Substring(0, 31) : sanitized;
    }

    public string GetMimeType() => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public string GetFileExtension() => ".xlsx";
}

