using HotelManagement.Application.Common.DTOs.Role;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Core.Role.Queries;

public record GetRoleByIdQuery : IRequest<RoleDto?>
{
    public required Guid Id { get; init; }
}

public class GetRoleByIdQueryValidator : AbstractValidator<GetRoleByIdQuery>
{
    public GetRoleByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Role ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Role ID cannot be an empty GUID.");
    }
}

public class GetRoleByIdQueryHandler(IRoleService roleService) : IRequestHandler<GetRoleByIdQuery, RoleDto?>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<RoleDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleService.GetRoleByIdAsync(request.Id);

        return role;
    }
}
