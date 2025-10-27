namespace HotelManagement.Application.Common.DTOs.Administrator;
public class AuditLogDto
{
    public Guid Id { get; set; }
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? UserEmail { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    public List<AuditLogDetailDto>? Changes { get; set; }
}
