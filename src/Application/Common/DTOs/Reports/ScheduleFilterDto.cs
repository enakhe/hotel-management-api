using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for filtering schedules
/// </summary>
public record ScheduleFilterDto
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public ReportType? ReportType { get; init; }
    public bool? IsActive { get; init; }
    public Guid? CreatedBy { get; init; }
    public Guid? TenantId { get; init; }
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}

