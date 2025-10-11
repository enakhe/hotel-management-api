using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Interfaces.Auth;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Auth.Commands;

public record ChangePasswordCommand : IRequest<Result>
{
    [Required]
    public required string UserId { get; set; }

    [Required, MinLength(8)]
    public required string CurrentPassword { get; set; }

    [Required, MinLength(8)]
    public required string NewPassword { get; set; }
}

public class ChangePasswordCommandHandler(IAuthService authService, IMapper mapper) : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IAuthService _authService = authService;
    private readonly IMapper _mapper = mapper;

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var changePasswordDto = _mapper.Map<ChangePasswordDto>(request);

        var response = await _authService.ChangePasswordAsync(changePasswordDto);

        return response;
    }
}
