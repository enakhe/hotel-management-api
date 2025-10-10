using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Interfaces.Auth;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Auth.Commands;

public record LoginCommand : IRequest<Result<AuthResponseDto>>
{
    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public required string Password { get; set; }
}

public class LoginCommandHandler(IAuthService authService, IMapper mapper) : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IAuthService _authService = authService;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = _mapper.Map<LoginRequestDto>(request);

        var loginResult = await _authService.LoginAsync(loginRequest);

        return loginResult;
    }
}
