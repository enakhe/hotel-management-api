namespace HotelManagement.Domain.Enums;

/// <summary>
/// Represents the billing cycle for subscription plans
/// </summary>
public enum BillingCycle
{
    /// <summary>
    /// Monthly billing cycle
    /// </summary>
    Monthly = 0,

    /// <summary>
    /// Yearly billing cycle
    /// </summary>
    Yearly = 1,

    /// <summary>
    /// One-time lifetime payment
    /// </summary>
    Lifetime = 2
}
