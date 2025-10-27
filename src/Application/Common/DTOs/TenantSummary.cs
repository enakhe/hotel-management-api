using HotelManagement.Domain.Entities.Configuration;
using HotelManagement.Domain.Entities.SuperAdmin;

namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Tenant summary for list views
/// </summary>
public record TenantSummary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public Plan? Plan { get; init; }
    public Domain.Entities.SuperAdmin.License? License { get; init; }
    public string? Country { get; init; }
    public string? Region { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastActivity { get; init; }
    public int UserCount { get; init; }
    public int BranchCount { get; init; }
}
