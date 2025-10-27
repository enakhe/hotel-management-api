namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Update tenant request
/// </summary>
public record UpdateTenantRequest
{
    public Guid TenantId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Address { get; init; }
    public string? ContactNumber { get; init; }
    public string? Email { get; init; }
    public string? TimeZone { get; init; }
    public string? CurrencyCode { get; init; }
    public string? LanguageCode { get; init; }
    public string? Country { get; init; }
    public string? Region { get; init; }
    public string? Industry { get; init; }
    public Guid PlanId { get; init; }
    public Guid LicenseId { get; init; }
}
