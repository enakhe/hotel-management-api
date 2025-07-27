using HotelManagement.Application.Common.DTOs.Role;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Core.Role.Queries;

public record GetAllRolesQuery : IRequest<List<RoleDto>>
{
}

public class GetAllRolesQueryValidator : AbstractValidator<GetAllRolesQuery>
{
    public GetAllRolesQueryValidator()
    {
    }
}

public class GetAllRolesQueryHandler(IRoleService roleService) : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<List<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetAllRolesAsync();
        return roles;
    }
}
