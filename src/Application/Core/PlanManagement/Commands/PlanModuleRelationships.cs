using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;
using HotelManagement.Application.Common.Interfaces.Services;

namespace HotelManagement.Application.Core.PlanManagement.Commands;

public record AssignModuleToPlanCommand : IRequest<Result<bool>>
{
    public Guid PlanId { get; init; }
    public Guid ModuleId { get; init; }
}

public class AssignModuleToPlanCommandValidator : AbstractValidator<AssignModuleToPlanCommand>
{
    public AssignModuleToPlanCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty()
            .WithMessage("Plan ID is required.");

        RuleFor(x => x.ModuleId)
            .NotEmpty()
            .WithMessage("Module ID is required.");
    }
}

public class AssignModuleToPlanCommandHandler(IPlanService service) : IRequestHandler<AssignModuleToPlanCommand, Result<bool>>
{
    private readonly IPlanService _service = service;

    public async Task<Result<bool>> Handle(AssignModuleToPlanCommand request, CancellationToken cancellationToken)
    {
        return await _service.AssignModuleToPlanAsync(request.PlanId, request.ModuleId);
    }
}

public record RemoveModuleFromPlanCommand : IRequest<Result<bool>>
{
    public Guid PlanId { get; init; }
    public Guid ModuleId { get; init; }
}

public class RemoveModuleFromPlanCommandValidator : AbstractValidator<RemoveModuleFromPlanCommand>
{
    public RemoveModuleFromPlanCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty()
            .WithMessage("Plan ID is required.");

        RuleFor(x => x.ModuleId)
            .NotEmpty()
            .WithMessage("Module ID is required.");
    }
}

public class RemoveModuleFromPlanCommandHandler(IPlanService service) : IRequestHandler<RemoveModuleFromPlanCommand, Result<bool>>
{
    private readonly IPlanService _service = service;

    public async Task<Result<bool>> Handle(RemoveModuleFromPlanCommand request, CancellationToken cancellationToken)
    {
        return await _service.RemoveModuleFromPlanAsync(request.PlanId, request.ModuleId);
    }
}
