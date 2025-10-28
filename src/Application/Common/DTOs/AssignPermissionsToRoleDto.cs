using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Application.Common.DTOs;

public class AssignPermissionsToRoleDto
{
    [Required]
    public Guid RoleId { get; set; }

    [Required]
    public List<Guid> PermissionIds { get; set; } = [];
}
