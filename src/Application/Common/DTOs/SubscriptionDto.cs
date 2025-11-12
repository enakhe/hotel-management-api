using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Request to create a new subscription
/// </summary>
public record CreateSubscriptionRequest
{
    public Guid TenantId { get; init; }
    public Guid PlanId { get; init; }
    public DateTime StartDate { get; init; } = DateTime.UtcNow;
    public DateTime? TrialEndDate { get; init; }
    public BillingCycle BillingCycle { get; init; }
    public decimal? CustomDiscount { get; init; }
    public string? DiscountReason { get; init; }
    public bool AutoRenew { get; init; } = true;
    public string? Notes { get; init; }
}

/// <summary>
/// Request to update a subscription
/// </summary>
public record UpdateSubscriptionRequest
{
    public DateTime? NextBillingDate { get; init; }
    public bool? AutoRenew { get; init; }
    public decimal? CustomDiscount { get; init; }
    public string? DiscountReason { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Subscription response DTO
/// </summary>
public record SubscriptionResponseDto
{
    public Guid Id { get; init; }
    public string SubscriptionNumber { get; init; } = string.Empty;
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public Guid PlanId { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public DateTime? TrialEndDate { get; init; }
    public SubscriptionStatus Status { get; init; }
    public string StatusDisplay { get; init; } = string.Empty;
    public BillingCycle BillingCycle { get; init; }
    public string BillingCycleDisplay { get; init; } = string.Empty;
    public decimal MonthlyPrice { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal? CustomDiscount { get; init; }
    public string? DiscountReason { get; init; }
    public DateTime NextBillingDate { get; init; }
    public DateTime? LastBillingDate { get; init; }
    public bool AutoRenew { get; init; }
    public string? Notes { get; init; }
    public List<SubscriptionModuleDto> Modules { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Module included in subscription
/// </summary>
public record SubscriptionModuleDto
{
    public Guid ModuleId { get; init; }
    public string ModuleName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool IsOptional { get; init; }
    public DateTime AddedAt { get; init; }
}

/// <summary>
/// Request to get subscriptions list
/// </summary>
public record GetSubscriptionsRequest
{
    public Guid? TenantId { get; init; }
    public SubscriptionStatus? Status { get; init; }
    public Guid? PlanId { get; init; }
    public DateTime? StartDateFrom { get; init; }
    public DateTime? StartDateTo { get; init; }
    public string? SearchTerm { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "CreatedAt";
    public bool SortDescending { get; init; } = true;
}


