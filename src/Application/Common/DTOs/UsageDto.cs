using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Usage summary for a billing period
/// </summary>
public record UsageSummaryDto
{
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    
    // Email usage
    public int EmailsSent { get; init; }
    public int EmailsIncluded { get; init; }
    public int EmailsOverage { get; init; }
    public decimal EmailOverageCost { get; init; }
    
    // SMS usage
    public int SmsSent { get; init; }
    public int SmsIncluded { get; init; }
    public int SmsOverage { get; init; }
    public decimal SmsOverageCost { get; init; }
    
    // Storage usage
    public decimal StorageUsedGB { get; init; }
    public decimal StorageIncludedGB { get; init; }
    public decimal StorageOverageGB { get; init; }
    public decimal StorageOverageCost { get; init; }
    
    // API usage
    public int ApiCallsMade { get; init; }
    public int ApiCallsIncluded { get; init; }
    public int ApiCallsOverage { get; init; }
    public decimal ApiCallsOverageCost { get; init; }
    
    public decimal TotalOverageCost { get; init; }
    public string Currency { get; init; } = "NGN";
}

/// <summary>
/// Overage calculation result
/// </summary>
public record OverageCalculationDto
{
    public Guid SubscriptionId { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public bool HasOverages { get; init; }
    public decimal TotalOverageCost { get; init; }
    public List<OverageItemDto> OverageItems { get; init; } = new();
    public string Currency { get; init; } = "NGN";
}

/// <summary>
/// Individual overage item
/// </summary>
public record OverageItemDto
{
    public UsageMetric Metric { get; init; }
    public string MetricDisplay { get; init; } = string.Empty;
    public decimal QuantityUsed { get; init; }
    public decimal QuantityIncluded { get; init; }
    public decimal QuantityOverage { get; init; }
    public decimal PricePerUnit { get; init; }
    public decimal TotalCost { get; init; }
}

/// <summary>
/// Limit check result
/// </summary>
public record LimitCheckDto
{
    public UsageMetric Metric { get; init; }
    public string MetricDisplay { get; init; } = string.Empty;
    public decimal CurrentUsage { get; init; }
    public decimal Limit { get; init; }
    public decimal Remaining { get; init; }
    public decimal PercentageUsed { get; init; }
    public bool IsOverLimit { get; init; }
    public bool IsNearLimit { get; init; } // >80% used
}

/// <summary>
/// Usage aggregation DTO
/// </summary>
public record UsageAggregationDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid SubscriptionId { get; init; }
    public DateTime BillingPeriodStart { get; init; }
    public DateTime BillingPeriodEnd { get; init; }
    
    public int EmailsSent { get; init; }
    public int EmailsIncluded { get; init; }
    public int EmailsOverage { get; init; }
    public decimal EmailOverageCost { get; init; }
    
    public int SmsSent { get; init; }
    public int SmsIncluded { get; init; }
    public int SmsOverage { get; init; }
    public decimal SmsOverageCost { get; init; }
    
    public decimal StorageUsedGB { get; init; }
    public decimal StorageIncludedGB { get; init; }
    public decimal StorageOverageGB { get; init; }
    public decimal StorageOverageCost { get; init; }
    
    public int ApiCallsMade { get; init; }
    public int ApiCallsIncluded { get; init; }
    public int ApiCallsOverage { get; init; }
    public decimal ApiCallsOverageCost { get; init; }
    
    public decimal TotalOverageCost { get; init; }
    public bool IsFinalized { get; init; }
    public bool IsBilled { get; init; }
}

