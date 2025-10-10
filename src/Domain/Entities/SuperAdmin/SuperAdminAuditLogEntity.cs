namespace HotelManagement.Domain.Entities.SuperAdmin;

/// <summary>
/// SuperAdmin audit log entity for database storage
/// </summary>
public class SuperAdminAuditLogEntity
{
    public Guid Id { get; set; }
    public Guid SuperAdminId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TargetType { get; set; } = string.Empty;
    public string? TargetId { get; set; }
    public Guid? TenantId { get; set; }
    public string? Details { get; set; }
    public string? Changes { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
}
