using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Auth;

namespace HotelManagement.Application.Auth.Commands;

public record RefreshTokenCommand : IRequest<TokenResponseDto>
{
    [Required]
    public required string RefreshToken { get; set; }
}

public class RefreshTokenCommandHandler(IAuthService authService) : IRequestHandler<RefreshTokenCommand, TokenResponseDto>
{
    private readonly IAuthService _authService = authService;

    public async Task<TokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenResponse = await _authService.RefreshTokenAsync(request.RefreshToken);

        return tokenResponse ?? throw new InvalidOperationException("Failed to refresh token. Please try again.");
    }
}
