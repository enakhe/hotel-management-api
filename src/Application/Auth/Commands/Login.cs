using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Auth;

namespace HotelManagement.Application.Auth.Commands;

public record LoginCommand(LoginRequestDto Login) : IRequest<AuthResponseDto>
{
}

public class LoginCommandHandler(IAuthService authService) : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IAuthService _authService = authService;

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var token = await _authService.LoginAsync(request.Login);

        return token;
    }
}
