using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Domain.Entities.Configuration;

namespace HotelManagement.Application.Common.Interfaces.Administrator;
public interface IBranchRepository
{
    Task AddAsync(Branch branch);
    Task UpdateAsync(Branch branch);
    Task DeleteAsync(Branch branch);
    Task<Branch?> GetByIdAsync(Guid branchId);
    Task<List<Branch>> GetAllAsync();
}
