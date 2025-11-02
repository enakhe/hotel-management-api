using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for report execution
/// </summary>
public record ReportExecutionDto
{
    public Guid Id { get; init; }
    public Guid? ReportScheduleId { get; init; }
    public Guid? ReportId { get; init; }
    public ReportStatus Status { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public long? DurationMs { get; init; }
    public int? RecordCount { get; init; }
    public string? ErrorMessage { get; init; }
    public Guid? InitiatedBy { get; init; }
    public bool IsScheduled { get; init; }
}

