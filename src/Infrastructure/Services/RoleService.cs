using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Infrastructure.Services;

public class RoleService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, ICacheService cache, ITenantContext tenantContext) : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ICacheService _cache = cache;
    private readonly ITenantContext _tenantContext = tenantContext;

    /// <summary>
    /// Invalidates cache for a specific role and tenant roles list
    /// </summary>
    private async Task InvalidateRoleCacheAsync(Guid roleId)
    {
        // Invalidate specific role cache
        await _cache.RemoveAsync($"role:{roleId}");
        
        // Invalidate tenant roles cache if tenant context is resolved
        if (_tenantContext.IsResolved && _tenantContext.TenantId.HasValue)
        {
            await _cache.RemoveAsync($"tenant:{_tenantContext.TenantId.Value}:roles");
        }
        else
        {
            await _cache.RemoveAsync("roles:all");
        }
    }

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

        // Invalidate role caches
        await InvalidateRoleCacheAsync(role.Id);

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

        // Invalidate user caches (roles changed)
        await _cache.RemoveAsync(CacheKeys.UserRoles(dto.UserId));
        await _cache.RemoveAsync(CacheKeys.User(dto.UserId));

        return !result.Succeeded ?
            Result.Failure(result.Errors.Select(e => e.Description), 400) :
            Result.Success(statusCode: 204);
    }

    public async Task<Result<List<RoleDto>>> GetAllRolesAsync()
    {
        // Try to get from cache (15 minutes - security sensitive)
        string cacheKey;
        if (_tenantContext.IsResolved && _tenantContext.TenantId.HasValue)
        {
            cacheKey = $"tenant:{_tenantContext.TenantId.Value}:roles";
        }
        else
        {
            cacheKey = "roles:all";
        }
        
        var cached = await _cache.GetAsync<List<RoleDto>>(cacheKey);
        if (cached != null)
        {
            return Result<List<RoleDto>>.Success(cached);
        }

        var roles = _roleManager.Roles.Select(role => new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            NormalizedName = role.NormalizedName!,
            Description = role.Description!
        }).ToList();

        var roleResult = await Task.FromResult(roles);

        // Cache for 15 minutes (security sensitive)
        await _cache.SetAsync(cacheKey, roleResult, TimeSpan.FromMinutes(15));

        return Result<List<RoleDto>>.Success(roleResult);
    }

    public async Task<Result<List<string>>> GetUserRolesAsync(Guid userId)
    {
        // Try to get from cache (15 minutes)
        var cacheKey = CacheKeys.UserRoles(userId);
        var cached = await _cache.GetAsync<List<string>>(cacheKey);
        
        if (cached != null)
        {
            return Result<List<string>>.Success(cached);
        }

        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new Application.Common.Exceptions.NotFoundException($"User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var rolesList = roles.ToList();

        // Cache for 15 minutes
        await _cache.SetAsync(cacheKey, rolesList, TimeSpan.FromMinutes(15));

        return Result<List<string>>.Success(rolesList);
    }

    public async Task<Result<RoleDto?>> GetRoleByIdAsync(Guid roleId)
    {
        // Try to get from cache (15 minutes)
        var cacheKey = $"role:{roleId}";
        var cached = await _cache.GetAsync<RoleDto>(cacheKey);
        
        if (cached != null)
        {
            return Result<RoleDto?>.Success(cached);
        }

        var role = await _roleManager.FindByIdAsync(roleId.ToString())
            ?? throw new Application.Common.Exceptions.NotFoundException($"Role not found.");

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name!,
            NormalizedName = role.NormalizedName!,
            Description = role.Description!
        };

        // Cache for 15 minutes
        await _cache.SetAsync(cacheKey, roleDto, TimeSpan.FromMinutes(15));

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

        // Invalidate role caches
        await InvalidateRoleCacheAsync(roleId);

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

        // Invalidate role caches
        await InvalidateRoleCacheAsync(roleId);

        return !result.Succeeded
            ? Result.Failure(result.Errors.Select(e => e.Description), 400)
            : Result.Success(statusCode: 204);
    }
}
