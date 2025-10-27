using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;
using HotelManagement.Application.Common.Interfaces.Services;

namespace HotelManagement.Application.Core.LicenseManagement.Commands;

public record ValidateLicenseCommand : IRequest<Result<LicenseValidationResponse>>
{
    public string LicenseKey { get; init; } = string.Empty;
    public string? HardwareId { get; init; }
    public string? Domain { get; init; }
    public string? IpAddress { get; init; }
    public string? ClientVersion { get; init; }
    public Guid? TenantId { get; init; }
}

public class ValidateLicenseCommandValidator : AbstractValidator<ValidateLicenseCommand>
{
    public ValidateLicenseCommandValidator()
    {
        RuleFor(x => x.LicenseKey)
            .NotEmpty()
            .WithMessage("License key is required.");

        RuleFor(x => x.Domain)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Domain))
            .WithMessage("Domain cannot exceed 255 characters.");

        RuleFor(x => x.IpAddress)
            .Must(BeValidIpAddress)
            .When(x => !string.IsNullOrEmpty(x.IpAddress))
            .WithMessage("Invalid IP address format.");

        RuleFor(x => x.ClientVersion)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.ClientVersion))
            .WithMessage("Client version cannot exceed 50 characters.");
    }

    private static bool BeValidIpAddress(string? ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return true;

        return System.Net.IPAddress.TryParse(ipAddress, out _);
    }
}

public class ValidateLicenseCommandHandler(ILicenseService service) : IRequestHandler<ValidateLicenseCommand, Result<LicenseValidationResponse>>
{
    private readonly ILicenseService _service = service;

    public async Task<Result<LicenseValidationResponse>> Handle(ValidateLicenseCommand request, CancellationToken cancellationToken)
    {
        var validationRequest = new LicenseValidationRequest
        {
            LicenseKey = request.LicenseKey,
            HardwareId = request.HardwareId,
            Domain = request.Domain,
            IpAddress = request.IpAddress,
            ClientVersion = request.ClientVersion,
            TenantId = request.TenantId
        };

        return await _service.ValidateLicenseAsync(validationRequest);
    }
}
