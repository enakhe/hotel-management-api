namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Create tenant result
/// </summary>
public record CreateTenantResult
{
    public bool Success { get; init; }
    public Guid? TenantId { get; init; }
    public string? ErrorMessage { get; init; }
    public string? InitialPassword { get; init; }
    public string? AdminEmail { get; init; }
}
