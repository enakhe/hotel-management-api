namespace HotelManagement.Domain.Enums;

/// <summary>
/// Represents the support level provided with subscription plans
/// </summary>
public enum SupportLevel
{
    /// <summary>
    /// Basic support level - email support only
    /// </summary>
    Basic = 0,

    /// <summary>
    /// Standard support level - email and chat support
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Premium support level - priority support with phone
    /// </summary>
    Premium = 2,

    /// <summary>
    /// Enterprise support level - dedicated support manager
    /// </summary>
    Enterprise = 3
}
