using HotelManagement.Application.Branches.Commands;
using HotelManagement.Application.Common.DTOs.Administrator;

namespace HotelManagement.Application.Common.Validators.Administrator;
public class CreateBranchValidator : AbstractValidator<CreateBranchDto>
{
    public CreateBranchValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .NotNull()
            .WithMessage("Name is required")
            .MaximumLength(100);

        RuleFor(x => x.Address)
            .NotEmpty()
            .NotNull().WithMessage("Address field is requred")
            .MaximumLength(200);

        RuleFor(x => x.ContactNumber)
            .Matches("^[0-9+]*$")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactNumber))
            .WithMessage("The contact numner field must be a valid phone number");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("The email field must be a valid email address");

        RuleFor(x => x.TimeZone)
            .MaximumLength(50);

        RuleFor(x => x.CurrencyCode)
            .MaximumLength(10);
    }
}
