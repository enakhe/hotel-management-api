using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Tenant.Commands;

public record CreateTenantCommand : IRequest<Result<TenantSummary>>
{
    public required string Name { get; init; }
    public required string Identifier { get; init; }
    public string? Description { get; init; }
    public string? Email { get; init; }
    public string? ContactNumber { get; init; }
    public Guid PlanId { get; init; }
    public Guid LicenseId { get; init; }
    public string? Address { get; init; }
    public string? Country { get; init; }
    public string? Region { get; init; }
    public string? Industry { get; init; }
    public string? TimeZone { get; init; } = "WAT";
    public string? CurrencyCode { get; init; } = "NGN";
    public string? LanguageCode { get; init; } = "en";
}

public class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tenant name is required.")
            .MaximumLength(100).WithMessage("Tenant name cannot exceed 100 characters.");

        RuleFor(x => x.Identifier)
            .NotEmpty()
            .WithMessage("Tenant identifier is required.")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Identifier can only contain letters, numbers, underscores, and hyphens.")
            .MaximumLength(50).WithMessage("Identifier cannot exceed 50 characters.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format.");

        RuleFor(x => x.ContactNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.ContactNumber))
            .WithMessage("Invalid contact number format.");

        RuleFor(x => x.PlanId)
            .NotEmpty()
            .WithMessage("Plan ID is required.");

        RuleFor(x => x.LicenseId)
            .NotEmpty()
            .WithMessage("License ID is required.");
    }
}

public class CreateTenantCommandHandler(ITenantService service, IMapper mapper) : IRequestHandler<CreateTenantCommand, Result<TenantSummary>>
{
    private readonly ITenantService _service = service;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<TenantSummary>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = _mapper.Map<CreateTenantRequest>(request);

        return await _service.CreateTenantAsync(tenant);
    }
}
