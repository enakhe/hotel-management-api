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

        var createdRoleResult = await GetRoleByIdAsync(role.Id);

        return !createdRoleResult.Succeeded || createdRoleResult.Data == null
            ? Result<RoleDto>.Failure("Failed to retrieve created role.", 500)
            : Result<RoleDto>.Success(createdRoleResult.Data, 201);
    }

    public async Task<Result> AssignRoleToUserAsync(AssignRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString())
            ?? throw new Application.Common.Exceptions.NotFoundException($"User not found.");

        var roleExists = await _roleManager.RoleExistsAsync(dto.RoleName);

        if (!roleExists)
            throw new Application.Common.Exceptions.NotFoundException($"Role not found.");

        var result = await _userManager.AddToRoleAsync(user, dto.RoleName);

        return !result.Succeeded ? 
            Result.Failure(result.Errors.Select(e => e.Description), 400) : 
            Result.Success(statusCode:204);
    }

    public async Task<Result<List<RoleDto>>> GetAllRolesAsync()
    {
        var roles = _roleManager.Roles.Select(role => new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            NormalizedName = role.NormalizedName!,
            Description = role.Description!
        }).ToList();

        var roleResult = await Task.FromResult(roles);
        return Result<List<RoleDto>>.Success(roleResult);
    }

    public async Task<Result<List<string>>> GetUserRolesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new Application.Common.Exceptions.NotFoundException($"User not found.");

        var roles = await _userManager.GetRolesAsync(user);

        return Result<List<string>>.Success([.. roles]);
    }

    public async Task<Result<RoleDto?>> GetRoleByIdAsync(Guid roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString())
            ?? throw new Application.Common.Exceptions.NotFoundException($"Role not found.");

        var roleDto =  new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            NormalizedName = role.NormalizedName!,
            Description = role.Description!
        };

        return Result<RoleDto?>.Success(roleDto);
    }

    public async Task<Result> UpdateRoleAsync(Guid roleId, CreateRoleDto updateRoleDto)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString())
            ?? throw new Application.Common.Exceptions.NotFoundException($"Role not found.");

        if (await _roleManager.RoleExistsAsync(updateRoleDto.Name) && role.Name != updateRoleDto.Name)
            throw new ConflictException($"Role with name '{updateRoleDto.Name}' already exists.");

        role.Name = updateRoleDto.Name;
        role.NormalizedName = updateRoleDto.Name.ToUpperInvariant();
        role.Description = updateRoleDto.Description;

        var result = await _roleManager.UpdateAsync(role);

        return !result.Succeeded 
            ? Result.Failure(result.Errors.Select(e => e.Description), 400) 
            : Result.Success(statusCode: 204);
    }

    public async Task<Result> DeleteRoleAsync(Guid roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString())
            ?? throw new Application.Common.Exceptions.NotFoundException($"Role not found.");

        if (role.Name == "Administrator")
            throw new InvalidOperationException("Cannot delete the Administrator role.");

        var result = await _roleManager.DeleteAsync(role);

        return !result.Succeeded
            ? Result.Failure(result.Errors.Select(e => e.Description), 400)
            : Result.Success(statusCode: 204);
    }
}
