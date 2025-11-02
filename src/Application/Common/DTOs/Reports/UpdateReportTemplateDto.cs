namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for updating a report template
/// </summary>
public record UpdateReportTemplateDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public Dictionary<string, object>? TemplateConfig { get; init; }
    public Dictionary<string, object>? CustomFields { get; init; }
    public bool? IsPublic { get; init; }
    public bool? IsActive { get; init; }
}

