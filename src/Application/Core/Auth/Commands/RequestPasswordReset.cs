using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Interfaces.Auth;

namespace HotelManagement.Application.Core.Auth.Commands;

public record RequestPasswordResetCommand : IRequest
{
    [Required, EmailAddress]
    public required string Email { get; set; }
}

public class RequestPasswordResetCommandHandler(IAuthService authService, IMapper mapper) : IRequestHandler<RequestPasswordResetCommand>
{
    private readonly IAuthService _authService = authService;
    private readonly IMapper _mapper = mapper;

    public async Task Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var resetRequestDto = _mapper.Map<ResetPasswordRequestDto>(request);

        await _authService.RequestPasswordResetAsync(resetRequestDto);
    }
}
