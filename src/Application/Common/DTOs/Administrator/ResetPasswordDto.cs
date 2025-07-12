using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
