using System.ComponentModel.DataAnnotations;
using HotelManagement.Domain.Entities.Data;
using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Entities.Configuration;
public class Branch : ITenantEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [Phone]
    public string? ContactNumber { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? TimeZone { get; set; }

    [MaxLength(10)]
    public string? CurrencyCode { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Tenant? Tenant { get; set; }
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
