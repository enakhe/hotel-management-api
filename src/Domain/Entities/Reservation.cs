using System.ComponentModel.DataAnnotations;
using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Entities.Hotel;

/// <summary>
/// Represents a hotel reservation
/// </summary>
public class Reservation : BaseBusinessEntity
{
    [Required]
    [MaxLength(50)]
    public required string ReservationNumber { get; set; }

    public Guid RoomId { get; set; }
    public Room? Room { get; set; }

    [Required]
    [MaxLength(100)]
    public required string GuestName { get; set; }

    [EmailAddress]
    public string? GuestEmail { get; set; }

    [Phone]
    public string? GuestPhone { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; } = 1;

    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }

    [MaxLength(10)]
    public string? Currency { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Confirmed;

    [MaxLength(500)]
    public string? SpecialRequests { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Payment information
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public DateTime? PaymentDate { get; set; }

    [MaxLength(100)]
    public string? PaymentMethod { get; set; }

    // Cancellation information
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public decimal? CancellationFee { get; set; }
}

/// <summary>
/// Reservation status enumeration
/// </summary>
public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    CheckedIn = 2,
    CheckedOut = 3,
    Cancelled = 4,
    NoShow = 5
}

/// <summary>
/// Payment status enumeration
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Partial = 1,
    Paid = 2,
    Refunded = 3,
    Failed = 4
}
