using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.Auth;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities.Configuration;

namespace HotelManagement.Application.Core.Tenant.Commands;

public record CreateTenantCommand : IRequest<Result<TenantSummary>>
{
    public required string Name { get; init; }
    public required string Identifier { get; init; }
    public string? Description { get; init; }
    public string? Email { get; init; }
    public string? ContactNumber { get; init; }
    public SubscriptionPlan SubscriptionPlan { get; init; } = SubscriptionPlan.Basic;
    public string[]? Modules { get; init; }
    public int MaxUsers { get; init; } = 10;
    public int MaxBranches { get; init; } = 1;
    public int MaxRooms { get; init; } = 100;
    public int MaxReservations { get; init; } = 1000;
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

        RuleFor(x => x.MaxUsers)
            .GreaterThan(0)
            .WithMessage("Max users must be greater than zero.");

        RuleFor(x => x.MaxBranches)
            .GreaterThan(0)
            .WithMessage("Max branches must be greater than zero.");

        RuleFor(x => x.MaxRooms)
            .GreaterThan(0)
            .WithMessage("Max rooms must be greater than zero.");

        RuleFor(x => x.MaxReservations)
            .GreaterThan(0)
            .WithMessage("Max reservations must be greater than zero.");
    }
}

public class CreateTenantCommandHandler(ISuperAdminService superAdminService, IMapper mapper) : IRequestHandler<CreateTenantCommand, Result<TenantSummary>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<TenantSummary>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = _mapper.Map<CreateTenantRequest>(request);

        return await _superAdminService.CreateTenantAsync(tenant);
    }
}
