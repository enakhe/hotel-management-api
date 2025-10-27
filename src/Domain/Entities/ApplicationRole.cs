using HotelManagement.Domain.Entities.Administrator;
using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Domain.Entities.Data;
public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public ICollection<RolePermission>? RolePermissions { get; set; }
}
