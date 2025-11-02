using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for report type information
/// </summary>
public record ReportTypeDto
{
    public ReportType Type { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public ReportCategory Category { get; init; }
    public List<ReportFormat> SupportedFormats { get; init; } = new();
    public Dictionary<string, string>? AvailableParameters { get; init; }
    public bool RequiresTenantContext { get; init; }
}

