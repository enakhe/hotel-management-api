namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Update tenant request
/// </summary>
public record UpdateTenantRequest
{
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
    public string? SubscriptionPlan { get; init; }
    public string[]? EnabledModules { get; init; }
    public int? MaxUsers { get; init; }
    public int? MaxBranches { get; init; }
    public int? MaxRooms { get; init; }
    public int? MaxReservations { get; init; }
}
