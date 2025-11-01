using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for updating a Tenant Administrator user profile
/// </summary>
public class UpdateTenantAdminDto
{
    [Required]
    public required Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public required string MiddleName { get; set; }

    [Required]
    [MaxLength(50)]
    public required string LastName { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }
}

