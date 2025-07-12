using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.Application.Common.DTOs.Auth;
public class ResetPasswordRequestDto
{
    [Required, EmailAddress]
    public required string Email { get; set; }
}
