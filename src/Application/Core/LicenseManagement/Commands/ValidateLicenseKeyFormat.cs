using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using HotelManagement.Application.Common.Services.LicenseKey;

namespace HotelManagement.Application.Core.LicenseManagement.Commands;

public record ValidateLicenseKeyFormatCommand : IRequest<Result<LicenseKeyValidationResponse>>
{
    public string LicenseKey { get; init; } = string.Empty;
    public LicenseKeyFormat ExpectedFormat { get; init; }
    public bool IncludeChecksum { get; init; } = true;
}

public record LicenseKeyValidationResponse
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public string MaskedKey { get; init; } = string.Empty;
    public string FormattedKey { get; init; } = string.Empty;
}

public class ValidateLicenseKeyFormatCommandValidator : AbstractValidator<ValidateLicenseKeyFormatCommand>
{
    public ValidateLicenseKeyFormatCommandValidator()
    {
        RuleFor(x => x.LicenseKey)
            .NotEmpty()
            .WithMessage("License key is required.");

        RuleFor(x => x.ExpectedFormat)
            .IsInEnum()
            .WithMessage("Invalid license key format.");
    }
}

public class ValidateLicenseKeyFormatCommandHandler : IRequestHandler<ValidateLicenseKeyFormatCommand, Result<LicenseKeyValidationResponse>>
{
    public Task<Result<LicenseKeyValidationResponse>> Handle(ValidateLicenseKeyFormatCommand request, CancellationToken cancellationToken)
    {
        var validationResult = LicenseKeyGeneratorService.ValidateDetailed(
                request.LicenseKey,
                request.ExpectedFormat,
                request.IncludeChecksum);

        var response = new LicenseKeyValidationResponse
        {
            IsValid = validationResult.IsValid,
            Errors = validationResult.Errors,
            Warnings = validationResult.Warnings,
            MaskedKey = LicenseKeyGeneratorService.MaskForDisplay(request.LicenseKey),
            FormattedKey = LicenseKeyGeneratorService.FormatForDisplay(request.LicenseKey)
        };

        return Task.FromResult(Result<LicenseKeyValidationResponse>.Success(response, 200));
    }
}
