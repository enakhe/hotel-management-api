namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for report subscription
/// </summary>
public record ReportSubscriptionDto
{
    public Guid Id { get; init; }
    public Guid SuperAdminId { get; init; }
    public required string Email { get; init; }
    public Guid ReportScheduleId { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastSentAt { get; init; }
    public int SentCount { get; init; }
}

