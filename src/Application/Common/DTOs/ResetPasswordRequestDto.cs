using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs.Auth;
public class ResetPasswordRequestDto
{
    [Required, EmailAddress]
    public required string Email { get; set; }
}
