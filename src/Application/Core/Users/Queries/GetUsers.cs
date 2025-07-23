using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Core.Users.Queries;

public record GetUsersQuery : IRequest<IEnumerable<UserDto>>
{
}

public class GetUsersQueryHandler(IUserService userService) : IRequestHandler<GetUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserService _userService = userService;

    public async Task<IEnumerable<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllUsersAsync();
        return users;
    }
}
