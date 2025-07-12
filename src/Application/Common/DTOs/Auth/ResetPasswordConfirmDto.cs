using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
