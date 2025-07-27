using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Administrator;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Role.Queries;

public record GetUserRolesQuery : IRequest<Result<List<string>>>
{
    [Required]
    public required Guid UserId { get; init; }
}

public class GetUserRolesQueryValidator : AbstractValidator<GetUserRolesQuery>
{
    public GetUserRolesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("User ID cannot be an empty GUID.");
    }
}

public class GetUserRolesQueryHandler(IRoleService roleService) : IRequestHandler<GetUserRolesQuery, Result<List<string>>>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<Result<List<string>>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var result = await _roleService.GetUserRolesAsync(request.UserId);

        return !result.Succeeded ?
            throw new ConflictException(string.Join("; ", result.Errors)) :
            result;
    }
}
