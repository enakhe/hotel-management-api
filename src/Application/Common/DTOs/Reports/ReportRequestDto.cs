using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for report generation request
/// </summary>
public record ReportRequestDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public ReportType Type { get; init; }
    public ReportCategory Category { get; init; }
    public ReportFormat Format { get; init; }
    public Guid? TenantId { get; init; }
    public ReportParametersDto? Parameters { get; init; }
    public Dictionary<string, object>? Filters { get; init; }
    public Guid? TemplateId { get; init; }
}

