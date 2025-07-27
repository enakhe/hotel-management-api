using HotelManagement.Application.Common.DTOs.Role;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Common.Interfaces.Administrator;
public interface IRoleService
{
    Task<Result<RoleDto>> CreateRoleAsync(CreateRoleDto dto);
    Task AssignRoleToUserAsync(AssignRoleDto assignRoleDto);
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<RoleDto?> GetRoleByIdAsync(Guid roleId);
    Task UpdateRoleAsync(Guid roleId, CreateRoleDto updateRoleDto);
    Task DeleteRoleAsync(Guid roleId);
}
