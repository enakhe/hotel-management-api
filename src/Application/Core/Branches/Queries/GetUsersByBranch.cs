using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Core.Branches.Queries;

public record GetUsersByBranchQuery : IRequest<IEnumerable<UserDto>>
{
    [Required]
    public required Guid Id { get; set; }
}

public class GetUsersByBranchQueryHandler(IBranchService branchService) : IRequestHandler<GetUsersByBranchQuery, IEnumerable<UserDto>>
{
    private readonly IBranchService _branchService = branchService;

    public async Task<IEnumerable<UserDto>> Handle(GetUsersByBranchQuery request, CancellationToken cancellationToken)
    {
        return await _branchService.GetUsersByBranchIdAsync(request.Id);
    }
}
