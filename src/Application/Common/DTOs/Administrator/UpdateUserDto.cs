using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.Application.Common.DTOs.Administrator;
public class UpdateUserDto
{
    [Required]
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string? FullName { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public Guid? BranchId { get; set; }

    public List<Guid> RoleIds { get; set; } = [];
}
