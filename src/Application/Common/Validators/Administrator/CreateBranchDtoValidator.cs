using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManagement.Application.Common.DTOs.Administrator;

namespace HotelManagement.Application.Common.Validators.Administrator;
public class CreateBranchDtoValidator : AbstractValidator<CreateBranchDto>
{
    public CreateBranchDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Address)
            .MaximumLength(200);

        RuleFor(x => x.ContactNumber)
            .Matches("^[0-9+]*$")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactNumber));

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.TimeZone)
            .MaximumLength(50);

        RuleFor(x => x.CurrencyCode)
            .MaximumLength(10);
    }
}
