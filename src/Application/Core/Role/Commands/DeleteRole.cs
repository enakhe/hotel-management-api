using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Core.Role.Commands;

public record DeleteRoleCommand : IRequest
{
    public required Guid Id { get; init; }
}

public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Role ID is required.");
    }
}

public class DeleteRoleCommandHandler(IRoleService roleService) : IRequestHandler<DeleteRoleCommand>
{
    private readonly IRoleService _roleService = roleService;

    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        await _roleService.DeleteRoleAsync(request.Id);
    }
}
