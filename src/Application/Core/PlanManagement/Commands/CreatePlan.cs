using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities.SuperAdmin;
using HotelManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using HotelManagement.Application.Common.Interfaces.Services;
using AutoMapper;

namespace HotelManagement.Application.Core.PlanManagement.Commands;

public record CreatePlanCommand : IRequest<Result<PlanResponseDto>>
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Currency { get; init; }
    public BillingCycle BillingCycle { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsPopular { get; init; } = false;
    public required CreatePlanLimitsCommand Limits { get; init; }
    public required Guid[] ModuleIds { get; init; }
}

public record CreatePlanLimitsCommand
{
    public int MaxUsers { get; init; }
    public int MaxBranches { get; init; }
    public int MaxRooms { get; init; }
    public int MaxReservations { get; init; }
    public int MaxStorageGB { get; init; }
    public int ApiRateLimit { get; init; }
    public SupportLevel SupportLevel { get; init; }
    public decimal SLA { get; init; }
}

public class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Plan name is required.")
            .MaximumLength(100)
            .WithMessage("Plan name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3)
            .WithMessage("Currency must be a 3-character code (e.g., USD, EUR).");

        RuleFor(x => x.BillingCycle)
            .IsInEnum()
            .WithMessage("Invalid billing cycle.");

        RuleFor(x => x.ModuleIds)
            .NotEmpty()
            .WithMessage("At least one module must be specified.");

        RuleFor(x => x.Limits)
            .NotNull()
            .WithMessage("Plan limits are required.");

        RuleFor(x => x.Limits)
            .SetValidator(new CreatePlanLimitsCommandValidator());
    }
}

public class CreatePlanLimitsCommandValidator : AbstractValidator<CreatePlanLimitsCommand>
{
    public CreatePlanLimitsCommandValidator()
    {
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

        RuleFor(x => x.MaxStorageGB)
            .GreaterThan(0)
            .WithMessage("Max storage must be greater than zero.");

        RuleFor(x => x.ApiRateLimit)
            .GreaterThan(0)
            .WithMessage("API rate limit must be greater than zero.");

        RuleFor(x => x.SupportLevel)
            .IsInEnum()
            .WithMessage("Invalid support level.");

        RuleFor(x => x.SLA)
            .InclusiveBetween(0, 100)
            .WithMessage("SLA must be between 0 and 100.");
    }
}

public class CreatePlanCommandHandler(IPlanService service, IMapper mapper) : IRequestHandler<CreatePlanCommand, Result<PlanResponseDto>>
{
    private readonly IPlanService _service = service;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PlanResponseDto>> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
    {
        var planRequest = _mapper.Map<CreatePlanRequest>(request);
        return await _service.CreatePlanAsync(planRequest);
    }
}
