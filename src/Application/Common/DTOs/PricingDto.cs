namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Price quote for a plan subscription
/// </summary>
public record PriceQuoteDto
{
    public Guid PlanId { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public decimal MonthlyPrice { get; init; }
    public decimal? SetupFee { get; init; }
    public decimal? Discount { get; init; }
    public decimal TaxRate { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalFirstMonth { get; init; }
    public decimal TotalRecurring { get; init; }
    public string Currency { get; init; } = string.Empty;
    public List<ModulePriceDto> Modules { get; init; } = new();
    public DateTime ValidUntil { get; init; }
}

/// <summary>
/// Module price detail
/// </summary>
public record ModulePriceDto
{
    public Guid ModuleId { get; init; }
    public string ModuleName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool IsRequired { get; init; }
}

/// <summary>
/// Detailed price breakdown
/// </summary>
public record PriceBreakdownDto
{
    public Guid PlanId { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public decimal BasePrice { get; init; }
    public decimal ModulesTotal { get; init; }
    public decimal? SetupFee { get; init; }
    public decimal? DiscountAmount { get; init; }
    public decimal? DiscountPercentage { get; init; }
    public decimal SubtotalBeforeTax { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = string.Empty;
    public List<ModulePriceDto> Modules { get; init; } = new();
}

/// <summary>
/// Proration calculation for plan changes
/// </summary>
public record ProrationDto
{
    public Guid CurrentPlanId { get; init; }
    public string CurrentPlanName { get; init; } = string.Empty;
    public decimal CurrentPlanPrice { get; init; }
    public Guid NewPlanId { get; init; }
    public string NewPlanName { get; init; } = string.Empty;
    public decimal NewPlanPrice { get; init; }
    public DateTime ChangeDate { get; init; }
    public int DaysRemaining { get; init; }
    public decimal UnusedAmount { get; init; }
    public decimal NewPlanProrata { get; init; }
    public decimal AmountDue { get; init; }
    public decimal CreditAmount { get; init; }
    public bool IsUpgrade { get; init; }
    public string Currency { get; init; } = string.Empty;
}


