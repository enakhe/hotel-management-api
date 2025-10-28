using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Role.Commands;

public record AssignRoleToUserCommand : IRequest<Result>
{
    public required Guid UserId { get; set; }
    public required string RoleName { get; set; }
}

public class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty.");

        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Role name is required.")
            .MaximumLength(100).WithMessage("Role name cannot exceed 100 characters.");
    }
}

public class AssignRoleToUserCommandHandler(IRoleService roleService, IMapper mapper) : IRequestHandler<AssignRoleToUserCommand, Result>
{
    private readonly IRoleService _roleService = roleService;
    private readonly IMapper _mapper = mapper;

    public async Task<Result> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var assignRoleDto = _mapper.Map<AssignRoleDto>(request);

        var result = await _roleService.AssignRoleToUserAsync(assignRoleDto);

        return !result.Succeeded ?
            throw new ConflictException(string.Join("; ", result.Errors)) :
            result;
    }
}
