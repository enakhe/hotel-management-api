using HotelManagement.Application.Common.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Infrastructure.Services.Reports.Exporters;

/// <summary>
/// PDF report exporter using QuestPDF
/// </summary>
public class PdfReportExporter : IReportExporter
{
    private readonly ILogger<PdfReportExporter> _logger;

    public PdfReportExporter(ILogger<PdfReportExporter> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> ExportAsync(ReportDataDto reportData, string reportName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting report to PDF: {ReportName}", reportName);

        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(container => ComposeHeader(container, reportData));
                    page.Content().Element(container => ComposeContent(container, reportData));
                    page.Footer().Element(container => ComposeFooter(container, reportData));
                });
            });

            return document.GeneratePdf();
        }, cancellationToken);
    }

    private void ComposeHeader(IContainer container, ReportDataDto reportData)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(reportData.Title).FontSize(20).Bold().FontColor(Colors.Blue.Darken2);

                if (!string.IsNullOrEmpty(reportData.Description))
                {
                    column.Item().Text(reportData.Description).FontSize(12).FontColor(Colors.Grey.Darken1);
                }

                column.Item().Text($"Generated: {reportData.GeneratedAt:yyyy-MM-dd HH:mm:ss}").FontSize(10).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private void ComposeContent(IContainer container, ReportDataDto reportData)
    {
        container.PaddingVertical(10).Column(column =>
        {
            foreach (var section in reportData.Sections)
            {
                column.Item().Element(c => ComposeSection(c, section));
                column.Item().PaddingTop(10);
            }

            if (reportData.Summary != null)
            {
                column.Item().Element(c => ComposeSummary(c, reportData.Summary));
            }
        });
    }

    private void ComposeSection(IContainer container, ReportSectionDto section)
    {
        container.Column(column =>
        {
            // Section title
            column.Item().PaddingBottom(5).Text(section.Title).FontSize(14).Bold().FontColor(Colors.Blue.Darken1);

            if (!string.IsNullOrEmpty(section.Description))
            {
                column.Item().PaddingBottom(5).Text(section.Description).FontSize(10).Italic().FontColor(Colors.Grey.Darken1);
            }

            // Tables
            foreach (var table in section.Tables)
            {
                column.Item().PaddingBottom(10).Element(c => ComposeTable(c, table));
            }
        });
    }

    private void ComposeTable(IContainer container, ReportTableDto table)
    {
        container.Column(column =>
        {
            if (!string.IsNullOrEmpty(table.Title))
            {
                column.Item().PaddingBottom(3).Text(table.Title).FontSize(12).SemiBold();
            }

            column.Item().Table(tableContainer =>
            {
                // Define columns
                tableContainer.ColumnsDefinition(columnsDefinition =>
                {
                    foreach (var _ in table.Columns)
                    {
                        columnsDefinition.RelativeColumn();
                    }
                });

                // Header
                tableContainer.Header(header =>
                {
                    foreach (var columnName in table.Columns)
                    {
                        header.Cell().Element(CellStyle).Text(columnName).SemiBold();
                    }

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten3).Padding(5);
                    }
                });

                // Rows
                foreach (var row in table.Rows)
                {
                    foreach (var cell in row)
                    {
                        tableContainer.Cell().Element(CellStyle).Text(cell?.ToString() ?? "");
                    }

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                    }
                }
            });
        });
    }

    private void ComposeSummary(IContainer container, ReportSummaryDto summary)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(10).Text("Summary").FontSize(14).Bold().FontColor(Colors.Blue.Darken1);

            column.Item().PaddingTop(5).Text($"Total Records: {summary.TotalRecords}").FontSize(11);

            if (summary.Statistics.Any())
            {
                column.Item().PaddingTop(5).Column(statsColumn =>
                {
                    foreach (var stat in summary.Statistics)
                    {
                        statsColumn.Item().Text($"{stat.Key}: {stat.Value}").FontSize(10);
                    }
                });
            }
        });
    }

    private void ComposeFooter(IContainer container, ReportDataDto reportData)
    {
        container.AlignCenter().DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Medium)).Text(text =>
        {
            text.Span("Page ");
            text.CurrentPageNumber();
            text.Span(" of ");
            text.TotalPages();
        });
    }

    public string GetMimeType() => "application/pdf";

    public string GetFileExtension() => ".pdf";
}

