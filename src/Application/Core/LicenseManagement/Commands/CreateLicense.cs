using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using AutoMapper;
using HotelManagement.Application.Common.Services.LicenseKey;
using HotelManagement.Application.Common.Interfaces.Services;

namespace HotelManagement.Application.Core.LicenseManagement.Commands;

public record CreateLicenseCommand : IRequest<Result<LicenseResponseDto>>
{
    public Guid TenantId { get; init; }
    public Guid PlanId { get; init; }
    public LicenseType Type { get; init; }
    public DateTime ExpirationDate { get; init; }
    public int? MaxValidations { get; init; }
    public string? HardwareId { get; init; }
    public string[]? DomainRestrictions { get; init; }
    public string[]? IpRestrictions { get; init; }
    public CreateLicenseFeatureCommand[]? Features { get; init; }
    public CreateLicenseLimitsCommand? Limits { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
    public string? CustomLicenseKey { get; init; }
}

public record CreateLicenseFeatureCommand
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool Enabled { get; init; }
    public int? Limit { get; init; }
    public string? Unit { get; init; }
    public Dictionary<string, object>? Configuration { get; init; }
}

public record CreateLicenseLimitsCommand
{
    public int MaxUsers { get; init; }
    public int MaxBranches { get; init; }
    public int MaxRooms { get; init; }
    public int MaxReservations { get; init; }
    public int MaxStorageGB { get; init; }
    public int ApiRateLimit { get; init; }
    public int ConcurrentSessions { get; init; }
    public Dictionary<string, int>? CustomLimits { get; init; }
}

public class CreateLicenseCommandValidator : AbstractValidator<CreateLicenseCommand>
{
    public CreateLicenseCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

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

        RuleForEach(x => x.Features)
            .SetValidator(new CreateLicenseFeatureCommandValidator())
            .When(x => x.Features != null && x.Features.Length > 0);

        RuleFor(x => x.Limits)
            .SetValidator(new CreateLicenseLimitsCommandValidator()!)
            .When(x => x.Limits != null);
    }
}

public class CreateLicenseFeatureCommandValidator : AbstractValidator<CreateLicenseFeatureCommand>
{
    public CreateLicenseFeatureCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Feature name is required and cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Feature description cannot exceed 500 characters.");

        RuleFor(x => x.Limit)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Limit.HasValue)
            .WithMessage("Feature limit must be non-negative.");

        RuleFor(x => x.Unit)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Unit))
            .WithMessage("Unit cannot exceed 50 characters.");
    }
}

public class CreateLicenseLimitsCommandValidator : AbstractValidator<CreateLicenseLimitsCommand>
{
    public CreateLicenseLimitsCommandValidator()
    {
        RuleFor(x => x.MaxUsers)
            .GreaterThanOrEqualTo(-1)
            .WithMessage("Max users must be non-negative or -1 for unlimited.");

        RuleFor(x => x.MaxBranches)
            .GreaterThanOrEqualTo(-1)
            .WithMessage("Max branches must be non-negative or -1 for unlimited.");

        RuleFor(x => x.MaxRooms)
            .GreaterThanOrEqualTo(-1)
            .WithMessage("Max rooms must be non-negative or -1 for unlimited.");

        RuleFor(x => x.MaxReservations)
            .GreaterThanOrEqualTo(-1)
            .WithMessage("Max reservations must be non-negative or -1 for unlimited.");

        RuleFor(x => x.MaxStorageGB)
            .GreaterThanOrEqualTo(-1)
            .WithMessage("Max storage must be non-negative or -1 for unlimited.");

        RuleFor(x => x.ApiRateLimit)
            .GreaterThanOrEqualTo(-1)
            .WithMessage("API rate limit must be non-negative or -1 for unlimited.");

        RuleFor(x => x.ConcurrentSessions)
            .GreaterThanOrEqualTo(-1)
            .WithMessage("Concurrent sessions must be non-negative or -1 for unlimited.");
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
