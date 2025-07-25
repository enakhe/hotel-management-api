using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManagement.Application.Common.DTOs.Role;

namespace HotelManagement.Application.Common.Services.Role;
public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetallRolesAsync();
    Task CreateRoleAsync(CreateRoleDto createRoleDto);
    Task AssignRoleToUserAsync(AssignRoleDto assignRoleDto);
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<RoleDto?> GetRoleByIdAsync(Guid roleId);
    Task UpdateRoleAsync(Guid roleId, CreateRoleDto updateRoleDto);
    Task DeleteRoleAsync(Guid roleId);
}
