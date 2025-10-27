namespace HotelManagement.Domain.Entities.Administrator;
public class Permission
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public ICollection<RolePermission>? RolePermissions { get; set; }
}
