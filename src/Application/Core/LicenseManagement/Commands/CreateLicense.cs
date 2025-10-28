using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Core.LicenseManagement.Commands;

public record CreateLicenseCommand : IRequest<Result<LicenseResponseDto>>
{
    public Guid PlanId { get; init; }
    public LicenseType Type { get; init; }
    public DateTime ExpirationDate { get; init; }
    public int? MaxValidations { get; init; }
    public string? HardwareId { get; init; }
    public string[]? DomainRestrictions { get; init; }
    public string[]? IpRestrictions { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
    public string? CustomLicenseKey { get; init; }
}

public class CreateLicenseCommandValidator : AbstractValidator<CreateLicenseCommand>
{
    public CreateLicenseCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty()
            .WithMessage("Plan ID is required.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid license type.");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Expiration date must be in the future.");

        RuleFor(x => x.MaxValidations)
            .GreaterThan(0)
            .When(x => x.MaxValidations.HasValue)
            .WithMessage("Max validations must be greater than 0.");
    }
}

public class CreateLicenseCommandHandler(ILicenseService service, IMapper mapper) : IRequestHandler<CreateLicenseCommand, Result<LicenseResponseDto>>
{
    private readonly ILicenseService _service = service;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<LicenseResponseDto>> Handle(CreateLicenseCommand request, CancellationToken cancellationToken)
    {
        var createRequest = _mapper.Map<CreateLicenseRequest>(request);

        // Auto-generate license key if not provided
        if (string.IsNullOrEmpty(createRequest.CustomLicenseKey))
        {
            var keyOptions = new Common.Services.LicenseKey.LicenseKeyGenerationOptions
            {
                Format = LicenseKeyFormat.XXXX_XXXX_XXXX_XXXX,
                IncludeChecksum = true
            };

            createRequest = createRequest with
            {
                CustomLicenseKey = Common.Services.LicenseKey.LicenseKeyGeneratorService.Generate(keyOptions)
            };
        }

        return await _service.CreateLicenseAsync(createRequest);
    }
}
