namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for report parameters
/// </summary>
public record ReportParametersDto
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? GroupBy { get; init; } // day, week, month, year
    public List<Guid>? TenantIds { get; init; }
    public List<string>? Regions { get; init; }
    public List<string>? Countries { get; init; }
    public List<Guid>? PlanIds { get; init; }
    public List<Guid>? LicenseIds { get; init; }
    public bool IncludeInactive { get; init; } = false;
    public bool IncludeDetails { get; init; } = true;
    public Dictionary<string, object>? CustomParameters { get; init; }
}

