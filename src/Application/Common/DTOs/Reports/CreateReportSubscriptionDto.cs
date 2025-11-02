namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for creating a report subscription
/// </summary>
public record CreateReportSubscriptionDto
{
    public Guid SuperAdminId { get; init; }
    public required string Email { get; init; }
    public Guid ReportScheduleId { get; init; }
}

