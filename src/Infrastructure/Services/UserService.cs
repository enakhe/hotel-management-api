using AutoMapper;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Services;

public class UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IMapper mapper, ICacheService cache, ITenantContext tenantContext) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly IMapper _mapper = mapper;
    private readonly ICacheService _cache = cache;
    private readonly ITenantContext _tenantContext = tenantContext;

    /// <summary>
    /// Invalidates cache for a specific user and tenant users list
    /// </summary>
    private async Task InvalidateUserCacheAsync(Guid userId)
    {
        await _cache.RemoveAsync(CacheKeys.User(userId));
        await _cache.RemoveAsync(CacheKeys.UserRoles(userId));
        await _cache.RemoveAsync(CacheKeys.UserPermissions(userId));
        
        // Invalidate tenant users cache if tenant context is resolved
        if (_tenantContext.IsResolved && _tenantContext.TenantId.HasValue)
        {
            await _cache.RemoveByPatternAsync($"tenant:{_tenantContext.TenantId.Value}:users:*");
        }
    }

    public async Task<Guid> CreateUserAsync(CreateUserDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "CreateUserDto cannot be null");

        if (dto.BranchId == null)
            throw new ArgumentNullException(nameof(dto), "BranchId cannot be null");

        var user = _mapper.Map<ApplicationUser>(dto)
            ?? throw new Exception("Failed to map CreateUserDto to ApplicationUser");

        user.Id = Guid.NewGuid();
        user.FirstName = dto.FirstName;
        user.MiddleName = dto.MiddleName;
        user.LastName = dto.LastName;
        user.FullName = $"{dto.FirstName} {dto.MiddleName} {dto.LastName}";
        user.BranchId = (Guid)dto.BranchId;

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new ConflictException(string.Join(", ", result.Errors.Select(e => e.Description)));

        if (dto.RoleIds != null && dto.RoleIds.Count > 0)
        {
            var roles = await _roleManager.Roles
                .Where(r => dto.RoleIds.Contains(r.Id))
                .Select(r => r.Name!)
                .ToListAsync();

            if (roles.Count != 0)
                await _userManager.AddToRolesAsync(user, roles);
        }

        // Invalidate user caches
        await InvalidateUserCacheAsync(user.Id);

        return user.Id;
    }

    public async Task UpdateUserAsync(UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.Id.ToString())
            ?? throw new Exception("User not found");

        user.FirstName = dto.FirstName;
        user.MiddleName = dto.MiddleName;
        user.LastName = dto.LastName;
        user.FullName = $"{dto.FirstName} {dto.MiddleName} {dto.LastName}";
        user.PhoneNumber = dto.PhoneNumber;
        user.Gender = dto.Gender;
        user.BranchId = dto.BranchId;
        user.LastUpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        if (dto.RoleIds != null && dto.RoleIds.Count > 0)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var newRoles = await _roleManager.Roles
                .Where(r => dto.RoleIds.Contains(r.Id))
                .Select(r => r.Name!)
                .ToListAsync();

            await _userManager.AddToRolesAsync(user, newRoles);
        }

        // Invalidate user caches
        await InvalidateUserCacheAsync(dto.Id);
    }

    public async Task DeactivateUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new Exception("User not found");

        user.IsActive = false;
        user.LastUpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        // Invalidate user caches
        await InvalidateUserCacheAsync(userId);
    }

    public async Task ActivateUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new Exception("User not found");

        user.IsActive = true;
        user.LastUpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        // Invalidate user caches
        await InvalidateUserCacheAsync(userId);
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new Exception("User not found");

        await _userManager.DeleteAsync(user);

        // Invalidate user caches
        await InvalidateUserCacheAsync(userId);
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        // Try to get from cache (15 minutes)
        var cacheKey = CacheKeys.User(userId);
        var cached = await _cache.GetAsync<UserDto>(cacheKey);
        
        if (cached != null)
        {
            return cached;
        }

        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new Exception("User not found");

        var dto = _mapper.Map<UserDto>(user);
        var roles = await _userManager.GetRolesAsync(user);

        dto.Roles = [.. roles];
        dto.BranchName = user.Branch?.Name;

        // Cache for 15 minutes
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15));

        return dto;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        // Try to get from cache (15 minutes)
        string cacheKey;
        if (_tenantContext.IsResolved && _tenantContext.TenantId.HasValue)
        {
            cacheKey = CacheKeys.TenantUsers(_tenantContext.TenantId.Value, 1, 1000); // Simple cache for all users
        }
        else
        {
            cacheKey = "users:all";
        }
        
        var cached = await _cache.GetAsync<List<UserDto>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var users = _userManager.Users.ToList();

        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var dto = _mapper.Map<UserDto>(user);
            dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            dto.BranchName = user.Branch?.Name;

            result.Add(dto);
        }

        // Cache for 15 minutes
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));

        return result;
    }

    public async Task AssignRolesAsync(Guid userId, List<Guid> roleIds)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new Exception("User not found");

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        var newRoles = await _roleManager.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name!)
            .ToListAsync();

        await _userManager.AddToRolesAsync(user, newRoles);

        // Invalidate user caches
        await InvalidateUserCacheAsync(userId);
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        // Try to get from cache (15 minutes)
        var cacheKey = CacheKeys.UserRoles(userId);
        var cached = await _cache.GetAsync<List<string>>(cacheKey);
        
        if (cached != null)
        {
            return cached;
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) throw new Exception("User not found");

        var roles = (await _userManager.GetRolesAsync(user)).ToList();

        // Cache for 15 minutes
        await _cache.SetAsync(cacheKey, roles, TimeSpan.FromMinutes(15));

        return roles;
    }

    public async Task ResetPasswordAsync(Guid userId, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new Exception("User not found");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email) != null;
    }

    public async Task<bool> PhoneExistsAsync(string phoneNumber)
    {
        return await Task.FromResult(_userManager.Users.Any(u => u.PhoneNumber == phoneNumber));
    }
}
