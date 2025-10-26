using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Domain.Entities.SuperAdmin;

/// <summary>
/// Represents the resource limits and constraints for a subscription plan
/// </summary>
public class PlanLimits
{
    public Guid Id { get; set; }

    /// <summary>
    /// Maximum number of users allowed
    /// </summary>
    public int MaxUsers { get; set; }

    /// <summary>
    /// Maximum number of branches allowed
    /// </summary>
    public int MaxBranches { get; set; }

    /// <summary>
    /// Maximum number of rooms allowed
    /// </summary>
    public int MaxRooms { get; set; }

    /// <summary>
    /// Maximum number of reservations allowed
    /// </summary>
    public int MaxReservations { get; set; }

    /// <summary>
    /// Maximum storage in GB
    /// </summary>
    public int MaxStorageGB { get; set; }

    /// <summary>
    /// API rate limit per hour
    /// </summary>
    public int ApiRateLimit { get; set; }

    /// <summary>
    /// Support level provided with this plan
    /// </summary>
    public SupportLevel SupportLevel { get; set; }

    /// <summary>
    /// Service Level Agreement percentage (e.g., 99.9 for 99.9% uptime)
    /// </summary>
    public decimal SLA { get; set; }

    /// <summary>
    /// Foreign key to the Plan these limits belong to
    /// </summary>
    public Guid PlanId { get; set; }

    /// <summary>
    /// Navigation property to the Plan
    /// </summary>
    [JsonIgnore]
    public Plan? Plan { get; set; }
}
