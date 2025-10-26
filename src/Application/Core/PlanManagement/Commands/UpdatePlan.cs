using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using AutoMapper;

namespace HotelManagement.Application.Core.PlanManagement.Commands;

public record UpdatePlanCommand : IRequest<Result<PlanResponseDto>>
{
    public Guid PlanId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
    public BillingCycle? BillingCycle { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsPopular { get; init; }
    public UpdatePlanFeatureCommand[]? Features { get; init; }
    public UpdatePlanLimitsCommand? Limits { get; init; }
    public string[]? Modules { get; init; }
}

public record UpdatePlanFeatureCommand
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? Included { get; init; }
    public int? Limit { get; init; }
    public string? Unit { get; init; }
}

public record UpdatePlanLimitsCommand
{
    public int? MaxUsers { get; init; }
    public int? MaxBranches { get; init; }
    public int? MaxRooms { get; init; }
    public int? MaxReservations { get; init; }
    public int? MaxStorageGB { get; init; }
    public int? ApiRateLimit { get; init; }
    public SupportLevel? SupportLevel { get; init; }
    public decimal? SLA { get; init; }
}

public class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
{
    public UpdatePlanCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty()
            .WithMessage("Plan ID is required.");

        RuleFor(x => x.Name)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Plan name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Price.HasValue)
            .WithMessage("Price must be a non-negative value.");

        RuleFor(x => x.Currency)
            .Length(3)
            .When(x => !string.IsNullOrEmpty(x.Currency))
            .WithMessage("Currency code must be 3 characters (e.g., USD).");

        RuleFor(x => x.BillingCycle)
            .IsInEnum()
            .When(x => x.BillingCycle.HasValue)
            .WithMessage("Invalid billing cycle.");

        RuleForEach(x => x.Features)
            .SetValidator(new UpdatePlanFeatureCommandValidator())
            .When(x => x.Features != null && x.Features.Length > 0);

        RuleFor(x => x.Limits)
            .SetValidator(new UpdatePlanLimitsCommandValidator())
            .When(x => x.Limits != null);
    }
}

public class UpdatePlanFeatureCommandValidator : AbstractValidator<UpdatePlanFeatureCommand>
{
    public UpdatePlanFeatureCommandValidator()
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
            .WithMessage("Limit must be a non-negative value.");

        RuleFor(x => x.Unit)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Unit))
            .WithMessage("Unit cannot exceed 50 characters.");
    }
}

public class UpdatePlanLimitsCommandValidator : AbstractValidator<UpdatePlanLimitsCommand?>
{
    public UpdatePlanLimitsCommandValidator()
    {
        RuleFor(x => x!.MaxUsers)
            .GreaterThanOrEqualTo(-1)
            .When(x => x != null && x.MaxUsers.HasValue)
            .WithMessage("Max users must be a non-negative value or -1 for unlimited.");

        RuleFor(x => x!.MaxBranches)
            .GreaterThanOrEqualTo(-1)
            .When(x => x != null && x.MaxBranches.HasValue)
            .WithMessage("Max branches must be a non-negative value or -1 for unlimited.");

        RuleFor(x => x!.MaxRooms)
            .GreaterThanOrEqualTo(-1)
            .When(x => x != null && x.MaxRooms.HasValue)
            .WithMessage("Max rooms must be a non-negative value or -1 for unlimited.");

        RuleFor(x => x!.MaxReservations)
            .GreaterThanOrEqualTo(-1)
            .When(x => x != null && x.MaxReservations.HasValue)
            .WithMessage("Max reservations must be a non-negative value or -1 for unlimited.");

        RuleFor(x => x!.MaxStorageGB)
            .GreaterThanOrEqualTo(-1)
            .When(x => x != null && x.MaxStorageGB.HasValue)
            .WithMessage("Max storage must be a non-negative value or -1 for unlimited.");

        RuleFor(x => x!.ApiRateLimit)
            .GreaterThanOrEqualTo(-1)
            .When(x => x != null && x.ApiRateLimit.HasValue)
            .WithMessage("API rate limit must be a non-negative value or -1 for unlimited.");

        RuleFor(x => x!.SupportLevel)
            .IsInEnum()
            .When(x => x != null && x.SupportLevel.HasValue)
            .WithMessage("Invalid support level.");

        RuleFor(x => x!.SLA)
            .InclusiveBetween(0, 100)
            .When(x => x != null && x.SLA.HasValue)
            .WithMessage("SLA must be between 0 and 100.");
    }
}

public class UpdatePlanCommandHandler(ISuperAdminService superAdminService, IMapper mapper) : IRequestHandler<UpdatePlanCommand, Result<PlanResponseDto>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PlanResponseDto>> Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = _mapper.Map<UpdatePlanRequest>(request);
        return await _superAdminService.UpdatePlanAsync(request.PlanId, updateRequest);
    }
}
