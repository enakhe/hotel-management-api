using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs.Administrator;
public class ResetPasswordDto
{
    [Required]
    public required Guid Id { get; set; }

    [Required, MinLength(8)]
    public required string Password { get; set; }

    [Required, MinLength(8)]
    public required string ConfirmPassword { get; set; }
}
