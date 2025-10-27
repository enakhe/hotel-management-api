using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs.Auth;
public class ResetPasswordConfirmDto
{
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Token { get; set; }

    [Required]
    [MinLength(8)]
    public required string Password { get; set; }
}
