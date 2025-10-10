namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Create tenant request
/// </summary>
public record CreateTenantRequest
{
    public string Name { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Address { get; init; }
    public string? ContactNumber { get; init; }
    public string? Email { get; init; }
    public string? TimeZone { get; init; } = "UTC";
    public string? CurrencyCode { get; init; } = "USD";
    public string? LanguageCode { get; init; } = "en";
    public string? Country { get; init; }
    public string? Region { get; init; }
    public string? Industry { get; init; }
    public string SubscriptionPlan { get; init; } = "Basic";
    public string[] EnabledModules { get; init; } = [];
    public int MaxUsers { get; init; } = 10;
    public int MaxBranches { get; init; } = 1;
    public int MaxRooms { get; init; } = 100;
    public int MaxReservations { get; init; } = 1000;
}
