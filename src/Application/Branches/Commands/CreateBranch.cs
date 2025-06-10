using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Branches.Commands;

public record CreateBranchCommand(CreateBranchDto Dto) : IRequest<Guid>
{
}

public class CreateBranchCommandHandler(IBranchService branchService) : IRequestHandler<CreateBranchCommand, Guid>
{
    private readonly IBranchService _branchService = branchService;

    public async Task<Guid> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var id = await _branchService.CreateBranchAsync(request.Dto);
        return id;
    }
}
