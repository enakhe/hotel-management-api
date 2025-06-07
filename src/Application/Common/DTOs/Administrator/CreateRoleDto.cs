using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.Application.Common.DTOs.Administrator;
public class CreateRoleDto
{
    [Required, MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public required string Description { get; set; }
}
