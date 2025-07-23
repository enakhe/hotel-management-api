using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Interfaces.Auth;

namespace HotelManagement.Application.Core.Auth.Commands;

public record ChangePasswordCommand : IRequest
{
    [Required]
    public required string UserId { get; set; }

    [Required, MinLength(8)]
    public required string CurrentPassword { get; set; }

    [Required, MinLength(8)]
    public required string NewPassword { get; set; }
}

public class ChangePasswordCommandHandler(IAuthService authService, IMapper mapper) : IRequestHandler<ChangePasswordCommand>
{
    private readonly IAuthService _authService = authService;
    private readonly IMapper _mapper = mapper;

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var changePasswordDto = _mapper.Map<ChangePasswordDto>(request);

        await _authService.ChangePasswordAsync(changePasswordDto);
    }
}
