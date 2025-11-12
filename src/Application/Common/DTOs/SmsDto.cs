namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// SMS response DTO
/// </summary>
public record SmsResponseDto
{
    public string MessageId { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
    public decimal? Cost { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// SMS delivery status DTO
/// </summary>
public record SmsDeliveryStatusDto
{
    public string MessageId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? DeliveredAt { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
}

