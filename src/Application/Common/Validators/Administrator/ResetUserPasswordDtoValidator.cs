using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelManagement.Application.Common.DTOs.Administrator;

namespace HotelManagement.Application.Common.Validators.Administrator;
public class ResetUserPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetUserPasswordDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Confirm Password is required.")
            .Equal(x => x.Password)
            .WithMessage("Confirm Password must match the Password.");
    }
}
