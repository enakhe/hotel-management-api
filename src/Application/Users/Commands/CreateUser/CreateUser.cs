using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Users.Commands.CreateUser;

public record CreateUserCommand(CreateUserDto Dto) : IRequest<Guid>{}

public class CreateUserCommandHandler(IUserService userService) : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserService _userService = userService;

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var id = await _userService.CreateUserAsync(request.Dto);
        return id;
    }
}
