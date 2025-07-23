using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.Auth;
using HotelManagement.Application.Common.Interfaces.Auth;

namespace HotelManagement.Application.Core.Auth.Commands;

public record ConfirmPasswordResetCommand : IRequest
{
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Token { get; set; }

    [Required]
    [MinLength(8)]
    public required string Password { get; set; }
}

public class ConfirmPasswordResetCommandValidator : AbstractValidator<ConfirmPasswordResetCommand>
{
    public ConfirmPasswordResetCommandValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("The email field must be a valid email address")
            .NotNull()
            .NotEmpty()
            .WithMessage("The email field is required");
        RuleFor(x => x.Token)
            .NotNull()
            .NotEmpty()
            .WithMessage("The token field is required");
        RuleFor(x => x.Password)
            .MinimumLength(8)
            .WithMessage("The password must be at least 8 characters long")
            .NotNull()
            .NotEmpty()
            .WithMessage("The password field is required");
    }
}

public class ConfirmPasswordResetCommandHandler(IAuthService authService, IMapper mapper) : IRequestHandler<ConfirmPasswordResetCommand>
{
    private readonly IAuthService _authService = authService;
    private readonly IMapper _mapper = mapper;

    public async Task Handle(ConfirmPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var confirmPasswordResetDto = _mapper.Map<ResetPasswordConfirmDto>(request);

        await _authService.ConfirmPasswordResetAsync(confirmPasswordResetDto);
    }
}
