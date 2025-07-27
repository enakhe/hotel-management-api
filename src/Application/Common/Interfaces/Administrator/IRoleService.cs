using HotelManagement.Application.Common.DTOs.Role;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Common.Interfaces.Administrator;
public interface IRoleService
{
    Task<Result<RoleDto>> CreateRoleAsync(CreateRoleDto dto);
    Task<Result> AssignRoleToUserAsync(AssignRoleDto dto);
    Task<Result<List<RoleDto>>> GetAllRolesAsync();
    Task<Result<List<string>>> GetUserRolesAsync(Guid userId);
    Task<Result<RoleDto?>> GetRoleByIdAsync(Guid roleId);
    Task<Result> UpdateRoleAsync(Guid roleId, CreateRoleDto updateRoleDto);
    Task<Result> DeleteRoleAsync(Guid roleId);
}
