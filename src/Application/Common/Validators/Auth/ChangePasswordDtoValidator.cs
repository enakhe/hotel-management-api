using HotelManagement.Application.Core.Auth.Commands;

namespace HotelManagement.Application.Common.Validators.Auth;
public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required.")
            .MinimumLength(6)
            .WithMessage("New password must be at least 6 characters long.");
    }
}
