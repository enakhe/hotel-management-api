using HotelManagement.Application.Common.DTOs.Role;
using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Application.Common.Interfaces.Administrator;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities.Data;
using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Infrastructure.Services.Administrator;
public class RoleService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager) : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result<RoleDto>> CreateRoleAsync(CreateRoleDto dto)
    {
        if (await _roleManager.RoleExistsAsync(dto.Name))
            throw new ConflictException($"Role with name '{dto.Name}' already exists.");

        var role = new ApplicationRole
        {
            Name = dto.Name,
            NormalizedName = dto.Name.ToUpperInvariant(),
            Description = dto.Description
        };

        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
            return Result<RoleDto>.Failure(result.Errors.Select(e => e.Description), 400);

        var createdRole = await GetRoleByIdAsync(role.Id);

        return Result<RoleDto>.Success(createdRole!, 201);
    }

    public async Task AssignRoleToUserAsync(AssignRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString())
            ?? throw new Application.Common.Exceptions.NotFoundException($"User not found.");

        var roleExists = await _roleManager.RoleExistsAsync(dto.RoleName);

        if (!roleExists)
            throw new Application.Common.Exceptions.NotFoundException($"Role not found.");

        var result = await _userManager.AddToRoleAsync(user, dto.RoleName);

        if (!result.Succeeded)
            throw new Exception($"Failed to assign role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        var roles = _roleManager.Roles.Select(role => new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            NormalizedName = role.NormalizedName!,
            Description = role.Description!
        }).ToList();

        return await Task.FromResult(roles);
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new Application.Common.Exceptions.NotFoundException($"User not found.");

        var roles = await _userManager.GetRolesAsync(user);

        return [.. roles];
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString())
            ?? throw new Application.Common.Exceptions.NotFoundException($"Role not found.");

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            NormalizedName = role.NormalizedName!,
            Description = role.Description!
        };
    }

    public async Task UpdateRoleAsync(Guid roleId, CreateRoleDto updateRoleDto)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString())
            ?? throw new Application.Common.Exceptions.NotFoundException($"Role not found.");

        if (await _roleManager.RoleExistsAsync(updateRoleDto.Name) && role.Name != updateRoleDto.Name)
            throw new ConflictException($"Role with name '{updateRoleDto.Name}' already exists.");

        role.Name = updateRoleDto.Name;
        role.NormalizedName = updateRoleDto.Name.ToUpperInvariant();
        role.Description = updateRoleDto.Description;

        var result = await _roleManager.UpdateAsync(role);

        if (!result.Succeeded)
            throw new Exception($"Failed to update role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }

    public async Task DeleteRoleAsync(Guid roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString())
            ?? throw new Application.Common.Exceptions.NotFoundException($"Role not found.");

        if (role.Name == "Administrator")
            throw new InvalidOperationException("Cannot delete the Administrator role.");

        var result = await _roleManager.DeleteAsync(role);

        if (!result.Succeeded)
            throw new Exception($"Failed to delete role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }
}
