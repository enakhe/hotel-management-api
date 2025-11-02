using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for updating a report schedule
/// </summary>
public record UpdateReportScheduleDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public ReportFormat? Format { get; init; }
    public ScheduleFrequency? Frequency { get; init; }
    public string? CronExpression { get; init; }
    public ReportParametersDto? Parameters { get; init; }
    public Dictionary<string, object>? Filters { get; init; }
    public List<string>? EmailRecipients { get; init; }
    public string? TimeZone { get; init; }
}

