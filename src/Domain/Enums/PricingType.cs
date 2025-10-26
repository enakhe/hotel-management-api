namespace HotelManagement.Domain.Enums;

/// <summary>
/// Types of pricing models for modules
/// </summary>
public enum PricingType
{
    /// <summary>
    /// Module is included in the plan at no extra cost
    /// </summary>
    Included = 0,

    /// <summary>
    /// Module is an add-on with a fixed price
    /// </summary>
    Addon = 1,

    /// <summary>
    /// Module pricing is per user
    /// </summary>
    PerUser = 2,

    /// <summary>
    /// Module pricing is based on usage
    /// </summary>
    PerUsage = 3
}
