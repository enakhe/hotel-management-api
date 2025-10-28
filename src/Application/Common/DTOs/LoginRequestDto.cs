using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs;

public class LoginRequestDto
{
    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}
