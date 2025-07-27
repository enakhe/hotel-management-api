using HotelManagement.Application.Common.DTOs.Role;
using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Administrator;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Role.Queries;

public record GetRoleByIdQuery : IRequest<Result<RoleDto?>>
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

public class GetRoleByIdQueryHandler(IRoleService roleService) : IRequestHandler<GetRoleByIdQuery, Result<RoleDto?>>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<Result<RoleDto?>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _roleService.GetRoleByIdAsync(request.Id);

        return !result.Succeeded 
            ? throw new ConflictException(string.Join("; ", result.Errors)) 
            : result;
    }
}
