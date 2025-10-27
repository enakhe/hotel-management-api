using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs.Auth;
public class ChangePasswordDto
{
    [Required]
    public required string UserId { get; set; }

    [Required, MinLength(8)]
    public required string CurrentPassword { get; set; }

    [Required, MinLength(8)]
    public required string NewPassword { get; set; }
}
