using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Core.Users.Queries;

public record GetUserRolesQuery : IRequest<IEnumerable<string>>
{
    [Required]
    public required Guid Id { get; set; }
}


public class GetUserRolesQueryHandler(IUserService userService) : IRequestHandler<GetUserRolesQuery, IEnumerable<string>>
{
    private readonly IUserService _userService = userService;

    public async Task<IEnumerable<string>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var userRoles = await _userService.GetUserRolesAsync(request.Id);

        return userRoles;
    }
}
