namespace HotelManagement.Domain.Entities;

public class RolePermission
{
    public Guid ApplicationRoleId { get; set; }
    public required ApplicationRole ApplicationRole { get; set; }

    public Guid PermissionId { get; set; }
    public required Permission Permission { get; set; }
}
