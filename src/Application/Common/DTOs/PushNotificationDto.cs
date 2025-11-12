namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Push notification response DTO
/// </summary>
public record PushNotificationResponseDto
{
    public string MessageId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime SentAt { get; init; }
    public int RecipientsCount { get; init; }
    public string? ErrorMessage { get; init; }
}

