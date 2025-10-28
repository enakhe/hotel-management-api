using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Auth.Commands;

public record RefreshTokenCommand : IRequest<Result<TokenResponseDto>>
{
    [Required]
    public required string RefreshToken { get; set; }
}

public class RefreshTokenCommandHandler(IAuthService authService) : IRequestHandler<RefreshTokenCommand, Result<TokenResponseDto>>
{
    private readonly IAuthService _authService = authService;

    public async Task<Result<TokenResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenResponse = await _authService.RefreshTokenAsync(request.RefreshToken);

        return tokenResponse;
    }
}
