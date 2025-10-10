namespace HotelManagement.Application.Common.DTOs.Tenant;

/// <summary>
/// Basic tenant information
/// </summary>
public record TenantInfo
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
