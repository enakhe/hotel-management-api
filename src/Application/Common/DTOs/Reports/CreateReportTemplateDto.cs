using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for creating a report template
/// </summary>
public record CreateReportTemplateDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public ReportType ReportType { get; init; }
    public Dictionary<string, object>? TemplateConfig { get; init; }
    public Dictionary<string, object>? CustomFields { get; init; }
    public bool IsPublic { get; init; } = false;
}

