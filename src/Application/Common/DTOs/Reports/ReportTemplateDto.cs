using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for report template
/// </summary>
public record ReportTemplateDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public ReportType ReportType { get; init; }
    public Dictionary<string, object>? TemplateConfig { get; init; }
    public Dictionary<string, object>? CustomFields { get; init; }
    public bool IsPublic { get; init; }
    public Guid CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public int UsageCount { get; init; }
    public bool IsActive { get; init; }
}

