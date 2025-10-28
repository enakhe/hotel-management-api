using HotelManagement.Application.Core.Auth.Commands;

namespace HotelManagement.Application.Common.Validators.Auth;

public class LoginDtoValidator : AbstractValidator<LoginCommand>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("The email field must be a valid email address")
            .NotNull()
            .NotEmpty()
            .WithMessage("The email field is required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .NotNull()
            .WithMessage("The password field is required");
    }
}
