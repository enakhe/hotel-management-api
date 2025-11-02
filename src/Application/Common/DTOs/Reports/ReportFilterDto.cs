using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for filtering reports
/// </summary>
public record ReportFilterDto
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public ReportType? Type { get; init; }
    public ReportStatus? Status { get; init; }
    public ReportFormat? Format { get; init; }
    public Guid? GeneratedBy { get; init; }
    public Guid? TenantId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}

