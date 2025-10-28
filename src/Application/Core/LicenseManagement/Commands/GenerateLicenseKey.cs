using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Common.Services.LicenseKey;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Core.LicenseManagement.Commands;

public record GenerateLicenseKeyCommand : IRequest<Result<LicenseKeyGenerationResponse>>
{
    public LicenseKeyFormat Format { get; init; }
    public string? Prefix { get; init; }
    public string? Suffix { get; init; }
    public bool IncludeChecksum { get; init; } = true;
    public string? CustomAlphabet { get; init; }
}

public record LicenseKeyGenerationResponse
{
    public string LicenseKey { get; init; } = string.Empty;
}

public class GenerateLicenseKeyCommandValidator : AbstractValidator<GenerateLicenseKeyCommand>
{
    public GenerateLicenseKeyCommandValidator()
    {
        RuleFor(x => x.Format)
            .IsInEnum()
            .WithMessage("Invalid license key format.");

        RuleFor(x => x.Prefix)
            .MaximumLength(10)
            .When(x => !string.IsNullOrEmpty(x.Prefix))
            .WithMessage("Prefix cannot exceed 10 characters.");

        RuleFor(x => x.Suffix)
            .MaximumLength(10)
            .When(x => !string.IsNullOrEmpty(x.Suffix))
            .WithMessage("Suffix cannot exceed 10 characters.");

        RuleFor(x => x.CustomAlphabet)
            .MinimumLength(10)
            .MaximumLength(62)
            .When(x => !string.IsNullOrEmpty(x.CustomAlphabet))
            .WithMessage("Custom alphabet must be between 10 and 62 characters.");
    }
}

public class GenerateLicenseKeyCommandHandler : IRequestHandler<GenerateLicenseKeyCommand, Result<LicenseKeyGenerationResponse>>
{
    public Task<Result<LicenseKeyGenerationResponse>> Handle(GenerateLicenseKeyCommand request, CancellationToken cancellationToken)
    {
        var options = new Common.Services.LicenseKey.LicenseKeyGenerationOptions
        {
            Format = request.Format,
            Prefix = request.Prefix,
            Suffix = request.Suffix,
            IncludeChecksum = request.IncludeChecksum,
            CustomAlphabet = request.CustomAlphabet
        };

        return Task.FromResult(Result<LicenseKeyGenerationResponse>.Success(new LicenseKeyGenerationResponse
        {
            LicenseKey = LicenseKeyGeneratorService.Generate(options)
        }));
    }
}
