using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.Application.Common.DTOs.Administrator;
public class AssignPermissionsToRoleDto
{
    [Required]
    public Guid RoleId { get; set; }

    [Required]
    public List<Guid> PermissionIds { get; set; } = [];
}
