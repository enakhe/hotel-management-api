using HotelManagement.Domain.Entities.Administrator;
using HotelManagement.Domain.Entities.Configuration;
using Microsoft.AspNetCore.Identity;
using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Entities.Data;
public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? TenantId { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? FullName { get; set; }
    public byte[]? ProfilePicture { get; set; }
    public GenderData Gender { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastUpdatedAt { get; set; } = DateTime.Now;

    public Guid BranchId { get; set; }
    public required Branch Branch { get; set; }

    // Navigation properties
    public Tenant? Tenant { get; set; }
    public ICollection<AuditLog>? AuditLogs { get; set; }
}
