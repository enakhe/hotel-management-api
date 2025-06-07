using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.Application.Common.DTOs.Administrator;
public class CreateUserDto
{
    [Required, MaxLength(100)]
    public required string FullName { get; set; }

    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required, MinLength(8)]
    public required string Password { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public Guid? BranchId { get; set; }

    public List<Guid> RoleIds { get; set; } = [];
}
