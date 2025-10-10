namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Tenant list request
/// </summary>
public record TenantListRequest
{
    public string? Query { get; init; }
    public string? Status { get; init; }
    public string? Plan { get; init; }
    public string? Region { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
    public string? SortBy { get; init; } = "CreatedAt";
    public bool SortDescending { get; init; } = true;
}
