using HotelManagement.Application.Common.DTOs.Administrator;

namespace HotelManagement.Application.Common.Interfaces.Administrator;
public interface IBranchService
{
    Guid CurrentBranchId { get; }
    Task<Guid> CreateBranchAsync(CreateBranchDto dto);
    Task UpdateBranchAsync(Guid id, CreateBranchDto dto);
    Task DeleteBranchAsync(Guid id);
    Task<BranchDto> GetBranchByIdAsync(Guid id);
    Task<IEnumerable<BranchDto>> GetAllBranchesAsync();
    Task<IEnumerable<UserDto>> GetUsersByBranchIdAsync(Guid branchId);
}
