using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for report data
/// </summary>
public record ReportDataDto
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public ReportType Type { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; init; }
    public List<ReportSectionDto> Sections { get; init; } = new();
    public ReportSummaryDto? Summary { get; init; }
}

/// <summary>
/// DTO for report section
/// </summary>
public record ReportSectionDto
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public List<ReportTableDto> Tables { get; init; } = new();
    public List<ReportChartDto> Charts { get; init; } = new();
    public Dictionary<string, object>? Data { get; init; }
}

/// <summary>
/// DTO for report table
/// </summary>
public record ReportTableDto
{
    public required string Title { get; init; }
    public List<string> Columns { get; init; } = new();
    public List<List<object>> Rows { get; init; } = new();
    public Dictionary<string, object>? Footer { get; init; }
}

/// <summary>
/// DTO for report chart
/// </summary>
public record ReportChartDto
{
    public required string Title { get; init; }
    public string ChartType { get; init; } = "bar"; // bar, line, pie, etc.
    public List<string> Labels { get; init; } = new();
    public List<ReportDatasetDto> Datasets { get; init; } = new();
}

/// <summary>
/// DTO for report dataset
/// </summary>
public record ReportDatasetDto
{
    public required string Label { get; init; }
    public List<object> Data { get; init; } = new();
    public string? Color { get; init; }
}

/// <summary>
/// DTO for report summary
/// </summary>
public record ReportSummaryDto
{
    public int TotalRecords { get; init; }
    public Dictionary<string, object> Statistics { get; init; } = new();
}

