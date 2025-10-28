namespace HotelManagement.Application.Common.DTOs;

public record PlanUsageDto
{
    public Guid PlanId { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public int TenantCount { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal AverageTenantValue { get; init; }
    public decimal ChurnRate { get; init; }
    public DateTime LastUpdated { get; init; }
}

public record ModuleUsageDto
{
    public Guid ModuleId { get; init; }
    public string ModuleName { get; init; } = string.Empty;
    public int TenantCount { get; init; }
    public decimal AdoptionRate { get; init; }
    public decimal Revenue { get; init; }
    public DateTime LastUpdated { get; init; }
}
