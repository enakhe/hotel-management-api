using FluentValidation;
using HotelManagement.Application.Auth.Commands;
using HotelManagement.Application.Common.DTOs.Auth;

namespace HotelManagement.Application.Common.Validators.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterCommand>
{
    public RegisterDtoValidator()
    {
        // Ensure the DTO object itself is not null
        RuleFor(x => x)
            .NotNull().WithMessage("Registration data cannot be null.");

        // Guard access to nested fields with .When(...)
        RuleFor(x => x.FirstName)
            .NotNull().WithMessage("First name is required.")
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50)
            .When(x => x != null);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .NotNull().WithMessage("Last name is required.")
            .MaximumLength(50)
            .When(x => x != null);

        RuleFor(x => x.PhoneNumber)
            .Matches("^[0-9+]*$").WithMessage("The phone number field must be a valid phone number.")
            .When(x => x != null && !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("The email field must be a valid email address")
            .MaximumLength(100)
            .When(x => x != null);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100)
            .When(x => x != null);

        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.")
            .Must(id => id.HasValue && id.Value != Guid.Empty)
            .WithMessage("Branch ID must be a valid GUID.")
            .When(x => x != null && x.BranchId.HasValue);
    }
}
