using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// DTO for resetting a Tenant Administrator's password
/// </summary>
public class ResetTenantAdminPasswordDto
{
    [Required]
    public required Guid UserId { get; set; }

    [Required]
    [MinLength(8)]
    public required string NewPassword { get; set; }
}

