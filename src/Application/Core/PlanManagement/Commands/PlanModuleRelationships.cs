using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;

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

public class AssignModuleToPlanCommandHandler(ISuperAdminService superAdminService) : IRequestHandler<AssignModuleToPlanCommand, Result<bool>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<bool>> Handle(AssignModuleToPlanCommand request, CancellationToken cancellationToken)
    {
        return await _superAdminService.AssignModuleToPlanAsync(request.PlanId, request.ModuleId);
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

public class RemoveModuleFromPlanCommandHandler(ISuperAdminService superAdminService) : IRequestHandler<RemoveModuleFromPlanCommand, Result<bool>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<bool>> Handle(RemoveModuleFromPlanCommand request, CancellationToken cancellationToken)
    {
        return await _superAdminService.RemoveModuleFromPlanAsync(request.PlanId, request.ModuleId);
    }
}
