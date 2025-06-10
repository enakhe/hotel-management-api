using HotelManagement.Application.Common.Interfaces.Administrator;
using HotelManagement.Domain.Entities.Data;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Repository;
public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<ApplicationUser?> GetByIdAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<ApplicationUser>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<List<ApplicationUser>> GetAllWithRolesAndBranchAsync()
    {
        return await _context.Users
            .Include(u => u.Branch)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> PhoneExistsAsync(string phoneNumber)
    {
        return await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
    }

    public async Task AddAsync(ApplicationUser user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ApplicationUser user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ApplicationUser user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public IQueryable<ApplicationUser> Query()
    {
        return _context.Users.AsQueryable();
    }

    public async Task<List<ApplicationUser>> GetUsersByBranchAsync(Guid branchId)
    {
        return await _context.Users
            .Where(u => u.BranchId == branchId)
            .ToListAsync();
    }

    public async Task<List<ApplicationUser>> SearchUsersAsync(string keyword)
    {
        return await _context.Users
            .Where(u => u.FullName!.Contains(keyword) ||
                        u.Email!.Contains(keyword) ||
                        u.PhoneNumber!.Contains(keyword))
            .ToListAsync();
    }

    public async Task<List<Guid>> GetUserIdsByRoleAsync(Guid roleId)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null) return [];

        var userIds = await (from ur in _context.UserRoles
                             join u in _context.Users on ur.UserId equals u.Id
                             where ur.RoleId == roleId
                             select u.Id).ToListAsync();

        return userIds;
    }
}
