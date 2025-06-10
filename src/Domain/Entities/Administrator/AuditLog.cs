using HotelManagement.Domain.Entities.Data;

namespace HotelManagement.Domain.Entities.Administrator;
public class AuditLog
{
    public Guid Id { get; set; }

    public required Guid UserId { get; set; }
    public required ApplicationUser User { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public required string Action { get; set; }
    public required string EntityName { get; set; }
    public required string EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public required ICollection<AuditLogDetail> PropertyChanges { get; set; }
}
