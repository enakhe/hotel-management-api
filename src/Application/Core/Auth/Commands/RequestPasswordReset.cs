using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Interfaces.Auth;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Auth.Commands;

public record RequestPasswordResetCommand : IRequest<Result<string>>
{
    [Required, EmailAddress]
    public required string Email { get; set; }
}

public class RequestPasswordResetCommandHandler(IAuthService authService, IMapper mapper) : IRequestHandler<RequestPasswordResetCommand, Result<string>>
{
    private readonly IAuthService _authService = authService;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<string>> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var resetRequestDto = _mapper.Map<ResetPasswordRequestDto>(request);

        var response = await _authService.RequestPasswordResetAsync(resetRequestDto);
        return response;
    }
}
