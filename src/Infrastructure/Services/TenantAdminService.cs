using AutoMapper;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Constants;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HotelManagement.Infrastructure.Services;

/// <summary>
/// Service for managing Tenant Administrator accounts via SuperAdmin control plane
/// </summary>
public class TenantAdminService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ApplicationDbContext context,
    ISuperAdminAuditService auditService,
    IMapper mapper,
    ILogger<TenantAdminService> logger) : ITenantAdminService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly ApplicationDbContext _context = context;
    private readonly ISuperAdminAuditService _auditService = auditService;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<TenantAdminService> _logger = logger;

    public async Task<Result<TenantAdminDto>> CreateTenantAdminAsync(CreateTenantAdminDto dto)
    {
        try
        {
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == dto.TenantId);

            if (tenant == null)
                return Result<TenantAdminDto>.Failure("Tenant not found", 404);

            if (!tenant.IsActive)
                return Result<TenantAdminDto>.Failure("Tenant is not active", 400);

            var hasAdmin = await ValidateOneTenantAdminAsync(dto.TenantId);
            if (!hasAdmin)
                return Result<TenantAdminDto>.Failure("Tenant already has an administrator", 400);

            var emailExists = await _userManager.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
                return Result<TenantAdminDto>.Failure($"Email '{dto.Email}' is already in use", 400);

            var branch = await GetOrCreateHeadquartersBranchAsync(tenant);

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true,
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,
                FullName = $"{dto.FirstName} {dto.MiddleName} {dto.LastName}",
                PhoneNumber = dto.PhoneNumber,
                TenantId = dto.TenantId,
                BranchId = branch.Id,
                Branch = branch,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create tenant admin: {Errors}", errors);
                return Result<TenantAdminDto>.Failure(errors, 400);
            }

            var adminRole = await _roleManager.FindByNameAsync(Roles.Administrator);
            if (adminRole != null)
            {
                await _userManager.AddToRoleAsync(user, Roles.Administrator);
            }
            else
            {
                _logger.LogWarning("Administrator role not found. Creating it...");
                var newAdminRole = new ApplicationRole
                {
                    Name = Roles.Administrator,
                    NormalizedName = Roles.Administrator.ToUpperInvariant(),
                    Description = "Tenant Administrator"
                };
                await _roleManager.CreateAsync(newAdminRole);
                await _userManager.AddToRoleAsync(user, Roles.Administrator);
            }

            await _auditService.LogActionAsync(
                "CreateTenantAdmin",
                "TenantAdmin",
                user.Id.ToString(),
                dto.TenantId,
                $"Created tenant administrator '{user.FullName}' ({user.Email}) for tenant '{tenant.Name}'",
                JsonSerializer.Serialize(new { dto.Email, dto.TenantId, UserId = user.Id }));

            _logger.LogInformation("Created tenant admin {Email} for tenant {TenantId}", dto.Email, dto.TenantId);

            return await GetTenantAdminByIdAsync(user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant admin for tenant {TenantId}", dto.TenantId);
            return Result<TenantAdminDto>.Failure($"An error occurred while creating tenant admin: {ex.Message}", 500);
        }
    }

    public async Task<Result<TenantAdminDto>> UpdateTenantAdminAsync(UpdateTenantAdminDto dto)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(dto.Id.ToString());
            if (user == null)
                return Result<TenantAdminDto>.Failure("Tenant admin not found", 404);

            var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Administrator);
            if (!isAdmin)
                return Result<TenantAdminDto>.Failure("User is not a tenant administrator", 400);

            user.FirstName = dto.FirstName;
            user.MiddleName = dto.MiddleName;
            user.LastName = dto.LastName;
            user.FullName = $"{dto.FirstName} {dto.MiddleName} {dto.LastName}";
            user.PhoneNumber = dto.PhoneNumber;
            user.LastUpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<TenantAdminDto>.Failure(errors, 400);
            }

            await _auditService.LogActionAsync(
                "UpdateTenantAdmin",
                "TenantAdmin",
                user.Id.ToString(),
                user.TenantId,
                $"Updated tenant administrator '{user.FullName}' ({user.Email})",
                JsonSerializer.Serialize(dto));

            _logger.LogInformation("Updated tenant admin {UserId}", dto.Id);

            return await GetTenantAdminByIdAsync(user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant admin {UserId}", dto.Id);
            return Result<TenantAdminDto>.Failure($"An error occurred while updating tenant admin: {ex.Message}", 500);
        }
    }

    public async Task<Result<TenantAdminDto>> GetTenantAdminByIdAsync(Guid userId)
    {
        try
        {
            var user = await _userManager.Users
                .Include(u => u.Tenant)
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Result<TenantAdminDto>.Failure("Tenant admin not found", 404);

            var roles = await _userManager.GetRolesAsync(user);

            var tenantAdminDto = new TenantAdminDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                TenantId = user.TenantId,
                TenantName = user.Tenant?.Name,
                TenantIdentifier = user.Tenant?.Identifier,
                BranchId = user.BranchId,
                BranchName = user.Branch?.Name,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt,
                LastUpdatedAt = user.LastUpdatedAt
            };

            return Result<TenantAdminDto>.Success(tenantAdminDto, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant admin {UserId}", userId);
            return Result<TenantAdminDto>.Failure($"An error occurred while retrieving tenant admin: {ex.Message}", 500);
        }
    }

    public async Task<Result<PaginatedResult<TenantAdminDto>>> GetTenantAdminsAsync(
        int page = 1,
        int pageSize = 20,
        Guid? tenantId = null,
        string? email = null,
        bool? isActive = null)
    {
        try
        {
            var adminRole = await _context.Roles
                .Where(r => r.Name == Roles.Administrator)
                .FirstOrDefaultAsync();

            if (adminRole == null)
                return Result<PaginatedResult<TenantAdminDto>>.Success(
                    new PaginatedResult<TenantAdminDto>
                    {
                        Items = new List<TenantAdminDto>(),
                        TotalCount = 0,
                        Page = page,
                        Size = pageSize
                    }, 200);

            var adminRoleId = adminRole.Id;

            var query = _userManager.Users
                .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId));

            if (tenantId.HasValue)
                query = query.Where(u => u.TenantId == tenantId.Value);

            if (!string.IsNullOrEmpty(email))
                query = query.Where(u => u.Email != null && u.Email.Contains(email));

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            var totalCount = await query.CountAsync();

            // Get user IDs first
            var userIds = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => u.Id)
                .ToListAsync();

            // Return early if no users found
            if (!userIds.Any())
            {
                return Result<PaginatedResult<TenantAdminDto>>.Success(
                    new PaginatedResult<TenantAdminDto>
                    {
                        Items = new List<TenantAdminDto>(),
                        TotalCount = 0,
                        Page = page,
                        Size = pageSize
                    }, 200);
            }

            // Load users with their related entities separately
            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            // Load tenants for users that have TenantId
            var tenantIds = users.Where(u => u.TenantId.HasValue).Select(u => u.TenantId!.Value).Distinct().ToList();
            var tenants = tenantIds.Any()
                ? await _context.Tenants.Where(t => tenantIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id)
                : new Dictionary<Guid, Tenant>();

            // Load branches for users that have BranchId
            var branchIds = users.Where(u => u.BranchId.HasValue).Select(u => u.BranchId!.Value).Distinct().ToList();
            var branches = branchIds.Any()
                ? await _context.Branches.Where(b => branchIds.Contains(b.Id)).ToDictionaryAsync(b => b.Id)
                : new Dictionary<Guid, Branch>();

            var tenantAdminDtos = new List<TenantAdminDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // Get tenant info from dictionary if user has a tenant
                Tenant? tenant = null;
                if (user.TenantId.HasValue)
                    tenants.TryGetValue(user.TenantId.Value, out tenant);

                // Get branch info from dictionary if user has a branch
                Branch? branch = null;
                if (user.BranchId.HasValue)
                    branches.TryGetValue(user.BranchId.Value, out branch);

                tenantAdminDtos.Add(new TenantAdminDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    TenantId = user.TenantId,
                    TenantName = tenant?.Name,
                    TenantIdentifier = tenant?.Identifier,
                    BranchId = user.BranchId,
                    BranchName = branch?.Name,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt,
                    LastUpdatedAt = user.LastUpdatedAt
                });
            }

            var paginatedResult = new PaginatedResult<TenantAdminDto>
            {
                Items = tenantAdminDtos,
                TotalCount = totalCount,
                Page = page,
                Size = pageSize
            };
            return Result<PaginatedResult<TenantAdminDto>>.Success(paginatedResult, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant admins");
            return Result<PaginatedResult<TenantAdminDto>>.Failure($"An error occurred while retrieving tenant admins: {ex.Message}", 500);
        }
    }

    public async Task<Result<TenantAdminDto>> GetTenantAdminByTenantIdAsync(Guid tenantId)
    {
        try
        {
            var adminRoleId = await _context.Roles
                .Where(r => r.Name == Roles.Administrator)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (adminRoleId == Guid.Empty)
                return Result<TenantAdminDto>.Failure("Administrator role not found", 404);

            var user = await _userManager.Users
                .Include(u => u.Tenant)
                .Include(u => u.Branch)
                .Where(u => u.TenantId == tenantId &&
                           _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId))
                .FirstOrDefaultAsync();

            if (user == null)
                return Result<TenantAdminDto>.Failure("Tenant admin not found for this tenant", 404);

            var roles = await _userManager.GetRolesAsync(user);

            var tenantAdminDto = new TenantAdminDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                TenantId = user.TenantId,
                TenantName = user.Tenant?.Name,
                TenantIdentifier = user.Tenant?.Identifier,
                BranchId = user.BranchId,
                BranchName = user.Branch?.Name,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt,
                LastUpdatedAt = user.LastUpdatedAt
            };

            return Result<TenantAdminDto>.Success(tenantAdminDto, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant admin for tenant {TenantId}", tenantId);
            return Result<TenantAdminDto>.Failure($"An error occurred while retrieving tenant admin: {ex.Message}", 500);
        }
    }

    public async Task<Result<bool>> ResetPasswordAsync(ResetTenantAdminPasswordDto dto)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
                return Result<bool>.Failure("Tenant admin not found", 404);

            var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Administrator);
            if (!isAdmin)
                return Result<bool>.Failure("User is not a tenant administrator", 400);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Failure(errors, 400);
            }

            await _auditService.LogActionAsync(
                "ResetTenantAdminPassword",
                "TenantAdmin",
                user.Id.ToString(),
                user.TenantId,
                $"Reset password for tenant administrator '{user.FullName}' ({user.Email})",
                null);

            _logger.LogInformation("Reset password for tenant admin {UserId}", dto.UserId);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for tenant admin {UserId}", dto.UserId);
            return Result<bool>.Failure($"An error occurred while resetting password: {ex.Message}", 500);
        }
    }

    public async Task<Result<bool>> ActivateAsync(Guid userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Result<bool>.Failure("Tenant admin not found", 404);

            var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Administrator);
            if (!isAdmin)
                return Result<bool>.Failure("User is not a tenant administrator", 400);

            user.IsActive = true;
            user.LastUpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Failure(errors, 400);
            }

            await _auditService.LogActionAsync(
                "ActivateTenantAdmin",
                "TenantAdmin",
                user.Id.ToString(),
                user.TenantId,
                $"Activated tenant administrator '{user.FullName}' ({user.Email})",
                null);

            _logger.LogInformation("Activated tenant admin {UserId}", userId);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating tenant admin {UserId}", userId);
            return Result<bool>.Failure($"An error occurred while activating tenant admin: {ex.Message}", 500);
        }
    }

    public async Task<Result<bool>> DeactivateAsync(Guid userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Result<bool>.Failure("Tenant admin not found", 404);

            var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Administrator);
            if (!isAdmin)
                return Result<bool>.Failure("User is not a tenant administrator", 400);

            user.IsActive = false;
            user.LastUpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Failure(errors, 400);
            }

            await _auditService.LogActionAsync(
                "DeactivateTenantAdmin",
                "TenantAdmin",
                user.Id.ToString(),
                user.TenantId,
                $"Deactivated tenant administrator '{user.FullName}' ({user.Email})",
                null);

            _logger.LogInformation("Deactivated tenant admin {UserId}", userId);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating tenant admin {UserId}", userId);
            return Result<bool>.Failure($"An error occurred while deactivating tenant admin: {ex.Message}", 500);
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Result<bool>.Failure("Tenant admin not found", 404);

            // Verify user is actually a tenant admin
            var isAdmin = await _userManager.IsInRoleAsync(user, Roles.Administrator);
            if (!isAdmin)
                return Result<bool>.Failure("User is not a tenant administrator", 400);

            if (user.TenantId.HasValue)
            {
                var tenant = await _context.Tenants.FindAsync(user.TenantId.Value);
                if (tenant != null && tenant.IsActive)
                {
                    var hasOtherAdmin = !await ValidateOneTenantAdminAsync(user.TenantId.Value);
                    if (!hasOtherAdmin)
                    {
                        return Result<bool>.Failure(
                            "Cannot delete the only administrator for an active tenant. Deactivate the tenant first or assign another admin.",
                            400);
                    }
                }
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.Failure(errors, 400);
            }

            await _auditService.LogActionAsync(
                "DeleteTenantAdmin",
                "TenantAdmin",
                user.Id.ToString(),
                user.TenantId,
                $"Deleted tenant administrator '{user.FullName}' ({user.Email})",
                null);

            _logger.LogInformation("Deleted tenant admin {UserId}", userId);

            return Result<bool>.Success(true, 200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tenant admin {UserId}", userId);
            return Result<bool>.Failure($"An error occurred while deleting tenant admin: {ex.Message}", 500);
        }
    }

    public async Task<bool> ValidateOneTenantAdminAsync(Guid tenantId)
    {
        try
        {
            var adminRoleId = await _context.Roles
                .Where(r => r.Name == Roles.Administrator)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (adminRoleId == Guid.Empty)
                return true;

            var adminCount = await _userManager.Users
                .Where(u => u.TenantId == tenantId &&
                           _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId))
                .CountAsync();

            return adminCount == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating tenant admin count for tenant {TenantId}", tenantId);
            return false;
        }
    }

    /// <summary>
    /// Gets or creates a "Headquarters" branch for the tenant
    /// </summary>
    private async Task<Branch> GetOrCreateHeadquartersBranchAsync(Tenant tenant)
    {
        var existingBranch = await _context.Branches
            .Where(b => b.TenantId == tenant.Id && b.IsActive)
            .FirstOrDefaultAsync();

        if (existingBranch != null)
        {
            _logger.LogInformation("Using existing branch {BranchId} for tenant {TenantId}",
                existingBranch.Id, tenant.Id);
            return existingBranch;
        }

        var hqBranch = await _context.Branches
            .Where(b => b.TenantId == tenant.Id && b.Name == "Headquarters")
            .FirstOrDefaultAsync();

        if (hqBranch != null)
        {
            hqBranch.IsActive = true;
            _context.Branches.Update(hqBranch);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Reactivated Headquarters branch {BranchId} for tenant {TenantId}",
                hqBranch.Id, tenant.Id);
            return hqBranch;
        }

        var newBranch = new Branch
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            Name = "Headquarters",
            IsActive = true,
            TimeZone = tenant.TimeZone,
            CurrencyCode = tenant.CurrencyCode,
            Address = tenant.Address,
            Email = tenant.Email,
            ContactNumber = tenant.ContactNumber,
            CreatedAt = DateTime.UtcNow
        };

        _context.Branches.Add(newBranch);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created Headquarters branch {BranchId} for tenant {TenantId}",
            newBranch.Id, tenant.Id);

        return newBranch;
    }
}

