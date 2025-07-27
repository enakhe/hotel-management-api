using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Administrator;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Role.Commands;

public record DeleteRoleCommand : IRequest<Result>
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

public class DeleteRoleCommandHandler(IRoleService roleService) : IRequestHandler<DeleteRoleCommand, Result>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var result = await _roleService.DeleteRoleAsync(request.Id);

        return !result.Succeeded ?
            throw new ConflictException(string.Join("; ", result.Errors)) :
            result;
    }
}
