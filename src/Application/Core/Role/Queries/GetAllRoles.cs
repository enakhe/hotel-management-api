using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Role.Queries;

public record GetAllRolesQuery : IRequest<Result<List<RoleDto>>>
{
}

public class GetAllRolesQueryValidator : AbstractValidator<GetAllRolesQuery>
{
    public GetAllRolesQueryValidator()
    {
    }
}

public class GetAllRolesQueryHandler(IRoleService roleService) : IRequestHandler<GetAllRolesQuery, Result<List<RoleDto>>>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<Result<List<RoleDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var result = await _roleService.GetAllRolesAsync();
        return result;
    }
}
