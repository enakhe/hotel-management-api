using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for report response
/// </summary>
public record ReportResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public ReportType Type { get; init; }
    public ReportCategory Category { get; init; }
    public ReportFormat Format { get; init; }
    public ReportStatus Status { get; init; }
    public Guid GeneratedBy { get; init; }
    public DateTime GeneratedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? DownloadUrl { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public long? FileSize { get; init; }
    public int? RecordCount { get; init; }
    public string? ErrorMessage { get; init; }
    public long? GenerationDurationMs { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? ReportScheduleId { get; init; }
}

