using HotelManagement.Application.Common.DTOs.Role;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Core.Role.Commands;

public record AssignRoleToUserCommand : IRequest
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

public class AssignRoleToUserCommandHandler(IRoleService roleService, IMapper mapper) : IRequestHandler<AssignRoleToUserCommand>
{
    private readonly IRoleService _roleService = roleService;
    private readonly IMapper _mapper = mapper;

    public async Task Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var assignRoleDto = _mapper.Map<AssignRoleDto>(request);

        await _roleService.AssignRoleToUserAsync(assignRoleDto);
    }
}
