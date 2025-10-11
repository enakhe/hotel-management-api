using HotelManagement.Application.Core.Auth.Commands;

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

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("The email field must be a valid email address")
            .MaximumLength(100)
            .When(x => x != null);

        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.")
            .Must(id => id.HasValue && id.Value != Guid.Empty)
            .WithMessage("Branch ID must be a valid GUID.")
            .When(x => x != null && x.BranchId.HasValue);

        RuleFor(x => x.Tenant)
            .NotEmpty().WithMessage("Tenant is required.")
            .NotNull().WithMessage("Tenant is required.")
            .MaximumLength(100)
            .When(x => x != null);
    }
}
