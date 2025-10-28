namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Tenant usage statistics
/// </summary>
public record TenantUsage
{
    public int UserCount { get; init; }
    public int BranchCount { get; init; }
    public int RoomCount { get; init; }
    public int ReservationCount { get; init; }
    public int ActiveReservations { get; init; }
    public long StorageUsedBytes { get; init; }
    public int ApiCallsLast24h { get; init; }
    public int ApiCallsLast7d { get; init; }
    public int ApiCallsLast30d { get; init; }
    public DateTime LastActivity { get; init; }
}
