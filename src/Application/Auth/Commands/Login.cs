using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Auth;

namespace HotelManagement.Application.Auth.Commands;

public record LoginCommand : IRequest<AuthResponseDto>
{
    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}

public class LoginCommandHandler(IAuthService authService, IMapper mapper) : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IAuthService _authService = authService;
    private readonly IMapper _mapper = mapper;

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = _mapper.Map<LoginRequestDto>(request);

        var token = await _authService.LoginAsync(loginRequest);

        return token;
    }
}
