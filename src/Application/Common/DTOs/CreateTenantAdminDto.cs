using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for creating a Tenant Administrator user via SuperAdmin control plane
/// </summary>
public class CreateTenantAdminDto
{
    [Required]
    [MaxLength(50)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public required string MiddleName { get; set; }

    [Required]
    [MaxLength(50)]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    [Required]
    [MinLength(8)]
    public required string Password { get; set; }

    [Required]
    public required Guid TenantId { get; set; }
}

