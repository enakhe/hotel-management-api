using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.Administrator;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Administrator;

namespace HotelManagement.Application.Users.Commands;

public record ResetUserPasswordCommand(ResetPasswordDto Dto) : IRequest
{

}

public class ResetUserPasswordCommandHandler(IUserService userService) : IRequestHandler<ResetUserPasswordCommand>
{
    private readonly IUserService _userService = userService;

    public async Task Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        await _userService.ResetPasswordAsync(request.Dto.Id, request.Dto.Password);
    }
}
