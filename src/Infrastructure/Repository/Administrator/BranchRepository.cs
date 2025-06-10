using HotelManagement.Application.Common.Interfaces.Administrator;
using HotelManagement.Domain.Entities.Configuration;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Infrastructure.Repository.Administrator;
public class BranchRepository(ApplicationDbContext context) : IBranchRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task AddAsync(Branch branch)
    {
        await _context.Branches.AddAsync(branch);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Branch branch)
    {
        _context.Branches.Update(branch);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Branch branch)
    {
        _context.Branches.Remove(branch);
        await _context.SaveChangesAsync();
    }

    public async Task<Branch?> GetByIdAsync(Guid branchId)
    {
        return await _context.Branches.FirstOrDefaultAsync(b => b.Id == branchId);
    }

    public async Task<List<Branch>> GetAllAsync()
    {
        return await _context.Branches.ToListAsync();
    }
}
