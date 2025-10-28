using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;

namespace HotelManagement.Application.Core.Users.Commands;

public record UpdateUserCommand(UpdateUserDto Dto) : IRequest
{
}

public class UpdateUserCommandHandler(IUserService userService) : IRequestHandler<UpdateUserCommand>
{
    private readonly IUserService _userService = userService;

    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        await _userService.UpdateUserAsync(request.Dto);
    }
}
