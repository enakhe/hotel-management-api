using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Branches.Queries;

public record GetAllBranchesQuery : IRequest<IEnumerable<BranchDto>>
{
}

public class GetAllBranchesQueryHandler(IBranchService branchService) : IRequestHandler<GetAllBranchesQuery, IEnumerable<BranchDto>>
{
    private readonly IBranchService _branchService = branchService;

    public async Task<IEnumerable<BranchDto>> Handle(GetAllBranchesQuery request, CancellationToken cancellationToken)
    {
        return await _branchService.GetAllBranchesAsync();
    }
}
