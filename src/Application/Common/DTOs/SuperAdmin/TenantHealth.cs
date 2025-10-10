namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Tenant health status
/// </summary>
public record TenantHealth
{
    public bool IsHealthy { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime LastHeartbeat { get; init; }
    public double ErrorRate { get; init; }
    public int ErrorCountLast24h { get; init; }
    public string[] Issues { get; init; } = Array.Empty<string>();
    public DateTime CheckedAt { get; init; }
}
