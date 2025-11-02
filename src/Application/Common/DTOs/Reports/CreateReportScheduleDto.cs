using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for creating a report schedule
/// </summary>
public record CreateReportScheduleDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public ReportType ReportType { get; init; }
    public ReportFormat Format { get; init; }
    public ScheduleFrequency Frequency { get; init; }
    public string? CronExpression { get; init; }
    public ReportParametersDto? Parameters { get; init; }
    public Dictionary<string, object>? Filters { get; init; }
    public List<string>? EmailRecipients { get; init; }
    public Guid? TenantId { get; init; }
    public string TimeZone { get; init; } = "UTC";
}

