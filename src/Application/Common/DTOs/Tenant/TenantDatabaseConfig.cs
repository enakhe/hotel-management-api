namespace HotelManagement.Application.Common.DTOs.Tenant;

/// <summary>
/// Tenant database configuration
/// </summary>
public record TenantDatabaseConfig
{
    public bool UseSharedDatabase { get; init; } = true;
    public string? ConnectionString { get; init; }
    public string Provider { get; init; } = "SqlServer";
    public string? DatabaseName { get; init; }
}
