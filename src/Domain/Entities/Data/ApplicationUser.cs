using HotelManagement.Domain.Entities.Administrator;
using HotelManagement.Domain.Entities.Configuration;
using HotelManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Domain.Entities.Data;
public class ApplicationUser : IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? MiddleName { get; set; }
    public required string FullName { get; set; }
    public byte[]? ProfilePicture { get; set; }
    public GenderData Gender { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastUpdatedAt { get; set; } = DateTime.Now;

    public Guid BranchId { get; set; }
    public required Branch Branch { get; set; }

    public ICollection<AuditLog>? AuditLogs { get; set; }
}
