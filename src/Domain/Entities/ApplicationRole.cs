using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public ICollection<RolePermission>? RolePermissions { get; set; }
}
