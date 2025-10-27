using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using AutoMapper;
using HotelManagement.Application.Common.Interfaces.Services;

namespace HotelManagement.Application.Core.PlanManagement.Commands;

public record UpdatePlanCommand : IRequest<Result<PlanResponseDto>>
{
    public Guid PlanId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Currency { get; init; }
    public BillingCycle? BillingCycle { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsPopular { get; init; }
    public UpdatePlanLimitsCommand? Limits { get; init; }
    public UpdatePlanModuleCommand[]? Modules { get; init; }
}

public record UpdatePlanModuleCommand
{
    public Guid Id { get; init; }
    public bool IsRequired { get; init; } = false;
    public int DisplayOrder { get; init; } = 0;
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

        RuleFor(x => x.Currency)
            .Length(3)
            .When(x => !string.IsNullOrEmpty(x.Currency))
            .WithMessage("Currency code must be 3 characters (e.g., USD).");

        RuleFor(x => x.BillingCycle)
            .IsInEnum()
            .When(x => x.BillingCycle.HasValue)
            .WithMessage("Invalid billing cycle.");

        RuleForEach(x => x.Modules)
            .SetValidator(new UpdatePlanModuleCommandValidator())
            .When(x => x.Modules != null && x.Modules.Length > 0);

        RuleFor(x => x.Limits)
            .SetValidator(new UpdatePlanLimitsCommandValidator())
            .When(x => x.Limits != null);
    }
}

public class UpdatePlanModuleCommandValidator : AbstractValidator<UpdatePlanModuleCommand>
{
    public UpdatePlanModuleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Module ID is required.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Display order must be a non-negative value.");
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

public class UpdatePlanCommandHandler(IPlanService service, IMapper mapper) : IRequestHandler<UpdatePlanCommand, Result<PlanResponseDto>>
{
    private readonly IPlanService _service = service;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PlanResponseDto>> Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = _mapper.Map<UpdatePlanRequest>(request);
        return await _service.UpdatePlanAsync(request.PlanId, updateRequest);
    }
}
