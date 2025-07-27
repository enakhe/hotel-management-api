using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Core.Role.Queries;

public record GetUserRolesQuery : IRequest<List<string>>
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

public class GetUserRolesQueryHandler(IRoleService roleService) : IRequestHandler<GetUserRolesQuery, List<string>>
{
    private readonly IRoleService _roleService = roleService;

    public async Task<List<string>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetUserRolesAsync(request.UserId);
        
        return roles ?? [];
    }
}
