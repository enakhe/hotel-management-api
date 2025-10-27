using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Tenant response DTO
/// </summary>
public record TenantResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Address { get; init; }
    public string? ContactNumber { get; init; }
    public string? Email { get; init; }
    public string? TimeZone { get; init; }
    public string? CurrencyCode { get; init; }
    public string? LanguageCode { get; init; }
    public bool IsActive { get; init; }
    public DateTime? SubscriptionStartDate { get; init; }
    public DateTime? SubscriptionEndDate { get; init; }
    public Guid PlanId { get; init; }
    public PlanResponseDto Plan { get; init; } = null!;
    public Guid? OverrideLimitsId { get; init; }
    public LimitsResponseDto? OverrideLimits { get; init; }
    public LicenseStatusType LicenseStatus { get; init; }
    public DateTime? LicenseExpiryDate { get; init; }
    public DateTime? LastLicenseCheck { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string? LastModifiedBy { get; init; }
}


/// <summary>
/// Tenant analytics DTO
/// </summary>
public record TenantAnalyticsDto
{
    public int TotalTenants { get; init; }
    public int ActiveTenants { get; init; }
    public int SuspendedTenants { get; init; }
    public int TrialTenants { get; init; }
    public int PremiumTenants { get; init; }
    public int EnterpriseTenants { get; init; }
    public decimal AverageTenantValue { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal ChurnRate { get; init; }
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Tenant usage DTO
/// </summary>
public record TenantUsageDto
{
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public string PlanName { get; init; } = string.Empty;
    public int UserCount { get; init; }
    public int BranchCount { get; init; }
    public int RoomCount { get; init; }
    public int ReservationCount { get; init; }
    public decimal StorageUsedGB { get; init; }
    public int ApiCallsLast24h { get; init; }
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;
}
