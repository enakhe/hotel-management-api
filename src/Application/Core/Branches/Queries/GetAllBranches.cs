using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;

namespace HotelManagement.Application.Core.Branches.Queries;

public record GetAllBranchesQuery : IRequest<IEnumerable<BranchDto>>
{
}

public class GetAllBranchesQueryHandler(IBranchService branchService) : IRequestHandler<GetAllBranchesQuery, IEnumerable<BranchDto>>
{
    private readonly IBranchService _branchService = branchService;

    public async Task<IEnumerable<BranchDto>> Handle(GetAllBranchesQuery request, CancellationToken cancellationToken)
    {
        var branches = await _branchService.GetAllBranchesAsync();

        return branches;
    }
}
