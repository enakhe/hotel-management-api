using HotelManagement.Application.Common.DTOs.Auth;

namespace HotelManagement.Application.Common.Validators.Auth;
public class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("The email field must be a valid email address")
            .NotNull()
            .NotEmpty()
            .WithMessage("The email field is required");
    }
}
