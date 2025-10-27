using HotelManagement.Domain.Entities.Data;

namespace HotelManagement.Application.Common.Interfaces.Administrator;
public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(Guid userId);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<List<ApplicationUser>> GetAllAsync();
    Task<List<ApplicationUser>> GetAllWithRolesAndBranchAsync();
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneExistsAsync(string phoneNumber);
    Task AddAsync(ApplicationUser user);
    Task UpdateAsync(ApplicationUser user);
    Task DeleteAsync(ApplicationUser user);

    IQueryable<ApplicationUser> Query();

    // Optional advanced
    Task<List<ApplicationUser>> GetUsersByBranchAsync(Guid branchId);
    Task<List<ApplicationUser>> SearchUsersAsync(string keyword);
    Task<List<Guid>> GetUserIdsByRoleAsync(Guid roleId);
}
