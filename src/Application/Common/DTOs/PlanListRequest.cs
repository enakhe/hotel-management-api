namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Request DTO for listing plans with filtering and pagination
/// </summary>
public record PlanListRequest
{
    public string? Query { get; init; }
    public bool? IsActive { get; init; }
    public string? BillingCycle { get; init; }
    public decimal? PriceMin { get; init; }
    public decimal? PriceMax { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public string SortBy { get; init; } = "createdAt";
    public bool SortDescending { get; init; } = true;
}
