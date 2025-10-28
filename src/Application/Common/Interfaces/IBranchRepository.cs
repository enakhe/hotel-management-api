using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Common.Interfaces;

public interface IBranchRepository
{
    Task AddAsync(Branch branch);
    Task UpdateAsync(Branch branch);
    Task DeleteAsync(Branch branch);
    Task<Branch?> GetByIdAsync(Guid branchId);
    Task<List<Branch>> GetAllAsync();
}
