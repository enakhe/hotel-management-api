using HotelManagement.Application.Core.Users.Commands;

namespace HotelManagement.Application.Common.Validators.Administrator;
public class CreateUserDtoValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Dto.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .NotNull().WithMessage("First name is required.")
            .MaximumLength(50);

        RuleFor(x => x.Dto.LastName)
            .NotNull().WithMessage("Last name is required.")
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50);

        RuleFor(x => x.Dto.PhoneNumber)
            .Matches("^[0-9+]*$").When(x => !string.IsNullOrWhiteSpace(x.Dto.PhoneNumber))
            .WithMessage("The phone number field must be a valid phone number.");

        RuleFor(x => x.Dto.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("The email field must be a valid email address")
            .MaximumLength(100);

        RuleFor(x => x.Dto.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100);
    }
}
