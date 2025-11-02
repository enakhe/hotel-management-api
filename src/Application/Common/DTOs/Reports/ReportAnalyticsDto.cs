using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for report analytics
/// </summary>
public record ReportAnalyticsDto
{
    public int TotalReports { get; init; }
    public int CompletedReports { get; init; }
    public int FailedReports { get; init; }
    public int PendingReports { get; init; }
    public Dictionary<ReportType, int> ReportsByType { get; init; } = new();
    public Dictionary<ReportFormat, int> ReportsByFormat { get; init; } = new();
    public Dictionary<string, int> ReportsByDay { get; init; } = new();
    public List<ReportTypeUsageDto> MostGeneratedReports { get; init; } = new();
    public long AverageGenerationTimeMs { get; init; }
    public long TotalFileSize { get; init; }
    public int ActiveSchedules { get; init; }
    public int TotalSubscriptions { get; init; }
}

/// <summary>
/// DTO for report type usage
/// </summary>
public record ReportTypeUsageDto
{
    public ReportType Type { get; init; }
    public int Count { get; init; }
    public long AverageTimeMs { get; init; }
}

