using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagement.Application.Common.DTOs.Role;
public class AssignRoleDto
{
    public required Guid UserId { get; set; }
    public required Guid RoleId { get; set; }
}
