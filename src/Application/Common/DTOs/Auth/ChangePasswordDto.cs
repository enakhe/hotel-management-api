using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.Application.Common.DTOs.Auth;
public class ChangePasswordDto
{
    [Required, MinLength(8)]
    public required string CurrentPassword { get; set; }

    [Required, MinLength(8)]
    public required string NewPassword { get; set; }
}
