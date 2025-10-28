using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Domain.Entities;

/// <summary>
/// Represents a room in a hotel
/// </summary>
public class Room : BaseBusinessEntity
{
    [Required]
    [MaxLength(20)]
    public required string RoomNumber { get; set; }

    [MaxLength(100)]
    public string? RoomName { get; set; }

    [Required]
    [MaxLength(50)]
    public required string RoomType { get; set; } // Single, Double, Suite, etc.

    [MaxLength(200)]
    public string? Description { get; set; }

    public int Floor { get; set; }
    public int MaxOccupancy { get; set; } = 2;
    public decimal BaseRate { get; set; }

    [MaxLength(10)]
    public string? Currency { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;

    // Room amenities (stored as JSON for flexibility)
    public string? Amenities { get; set; }

    // Room status
    public RoomStatus Status { get; set; } = RoomStatus.Available;

    // Last maintenance information
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }

    // Navigation properties
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

/// <summary>
/// Room status enumeration
/// </summary>
public enum RoomStatus
{
    Available = 0,
    Occupied = 1,
    OutOfOrder = 2,
    Maintenance = 3,
    Cleaning = 4
}
