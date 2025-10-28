using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Core.LicenseManagement.Commands;

public record UpdateLicenseCommand : IRequest<Result<LicenseResponseDto>>
{
    public Guid LicenseId { get; init; }
    public LicenseStatus? Status { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public int? MaxValidations { get; init; }
    public string? HardwareId { get; init; }
    public string[]? DomainRestrictions { get; init; }
    public string[]? IpRestrictions { get; init; }
    public UpdateLicenseFeatureCommand[]? Features { get; init; }
    public UpdateLicenseLimitsCommand? Limits { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

public record UpdateLicenseFeatureCommand
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? Enabled { get; init; }
    public int? Limit { get; init; }
    public string? Unit { get; init; }
    public Dictionary<string, object>? Configuration { get; init; }
}

public record UpdateLicenseLimitsCommand
{
    public int? MaxUsers { get; init; }
    public int? MaxBranches { get; init; }
    public int? MaxRooms { get; init; }
    public int? MaxReservations { get; init; }
    public int? MaxStorageGB { get; init; }
    public int? ApiRateLimit { get; init; }
    public int? ConcurrentSessions { get; init; }
    public Dictionary<string, int>? CustomLimits { get; init; }
}

public class UpdateLicenseCommandValidator : AbstractValidator<UpdateLicenseCommand>
{
    public UpdateLicenseCommandValidator()
    {
        RuleFor(x => x.LicenseId)
            .NotEmpty()
            .WithMessage("License ID is required.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Invalid license status.");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpirationDate.HasValue)
            .WithMessage("Expiration date must be in the future.");

        RuleFor(x => x.MaxValidations)
            .GreaterThan(0)
            .When(x => x.MaxValidations.HasValue)
            .WithMessage("Max validations must be greater than 0.");

        RuleForEach(x => x.Features)
            .SetValidator(new UpdateLicenseFeatureCommandValidator())
            .When(x => x.Features != null && x.Features.Length > 0);

        RuleFor(x => x.Limits)
            .SetValidator(new UpdateLicenseLimitsCommandValidator())
            .When(x => x.Limits != null);
    }
}

public class UpdateLicenseFeatureCommandValidator : AbstractValidator<UpdateLicenseFeatureCommand>
{
    public UpdateLicenseFeatureCommandValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Feature name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
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

public class UpdateLicenseLimitsCommandValidator : AbstractValidator<UpdateLicenseLimitsCommand?>
{
    public UpdateLicenseLimitsCommandValidator()
    {
        RuleFor(x => x!.MaxUsers)
            .GreaterThanOrEqualTo(-1)
            .When(x => x != null && x.MaxUsers.HasValue)
            .WithMessage("Max users must be non-negative or -1 for unlimited.");

        RuleFor(x => x!.MaxBranches)
            .GreaterThanOrEqualTo(-1)
            .When(x => x != null && x.MaxBranches.HasValue)
            .WithMessage("Max branches must be non-negative or -1 for unlimited.");

        RuleFor(x => x!.MaxRooms)
            .GreaterThanOrEqualTo(-1)
            .When(x => x != null && x.MaxRooms.HasValue)
            .WithMessage("Max rooms must be non-negative or -1 for unlimited.");

        RuleFor(x => x!.MaxReservations)
            .GreaterThanOrEqualTo(-1)
            .When(x => x != null && x.MaxReservations.HasValue)
            .WithMessage("Max reservations must be non-negative or -1 for unlimited.");

        RuleFor(x => x!.MaxStorageGB)
            .GreaterThanOrEqualTo(-1)
            .When(x => x != null && x.MaxStorageGB.HasValue)
            .WithMessage("Max storage must be non-negative or -1 for unlimited.");

        RuleFor(x => x!.ApiRateLimit)
            .GreaterThanOrEqualTo(-1)
            .When(x => x != null && x.ApiRateLimit.HasValue)
            .WithMessage("API rate limit must be non-negative or -1 for unlimited.");

        RuleFor(x => x!.ConcurrentSessions)
            .GreaterThanOrEqualTo(-1)
            .When(x => x != null && x.ConcurrentSessions.HasValue)
            .WithMessage("Concurrent sessions must be non-negative or -1 for unlimited.");
    }
}

public class UpdateLicenseCommandHandler(ILicenseService service, IMapper mapper) : IRequestHandler<UpdateLicenseCommand, Result<LicenseResponseDto>>
{
    private readonly ILicenseService _service = service;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<LicenseResponseDto>> Handle(UpdateLicenseCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = _mapper.Map<UpdateLicenseRequest>(request);
        return await _service.UpdateLicenseAsync(request.LicenseId, updateRequest);
    }
}
