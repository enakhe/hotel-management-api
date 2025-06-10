using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Application.Common.Interfaces.Administrator;
using HotelManagement.Domain.Entities.Data;
using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Application.Common.Services.Administrator;
public class UserService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IMapper mapper) : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly IMapper _mapper = mapper;

    public async Task<Guid> CreateUserAsync(CreateUserDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "CreateUserDto cannot be null");

        // Fix for CS8629: Nullable value type may be null.
        if (dto.BranchId == null)
            throw new ArgumentNullException(nameof(dto.BranchId), "BranchId cannot be null");

        var user = _mapper.Map<ApplicationUser>(dto) ?? throw new Exception("Failed to map CreateUserDto to ApplicationUser");

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

        return user.Id;
    }

    public async Task UpdateUserAsync(UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.Id.ToString()) ?? throw new Exception("User not found");

        user.FirstName = dto.FirstName;
        user.MiddleName = dto.MiddleName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.ProfilePicture = dto.ProfilePicture;
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
    }

    public async Task DeactivateUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new Exception("User not found");

        user.IsActive = false;
        user.LastUpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new Exception("User not found");

        await _userManager.DeleteAsync(user);
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new Exception("User not found");

        var dto = _mapper.Map<UserDto>(user);
        var roles = await _userManager.GetRolesAsync(user);

        dto.Roles = [.. roles];
        dto.BranchName = user.Branch?.Name;

        return dto;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = _userManager.Users.ToList();

        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var dto = _mapper.Map<UserDto>(user);
            dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            dto.BranchName = user.Branch?.Name;

            result.Add(dto);
        }

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
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        return user == null ? throw new Exception("User not found") : [.. await _userManager.GetRolesAsync(user)];
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
