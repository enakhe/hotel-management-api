using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for filtering report history
/// </summary>
public record ReportHistoryFilterDto
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public Guid? ReportScheduleId { get; init; }
    public Guid? InitiatedBy { get; init; }
    public ReportStatus? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public bool ScheduledOnly { get; init; } = false;
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}

