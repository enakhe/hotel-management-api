using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace HotelManagement.Application.Core.PlanManagement.Commands;

public record DeletePlanCommand : IRequest<Result<bool>>
{
    public Guid PlanId { get; init; }
}

public class DeletePlanCommandValidator : AbstractValidator<DeletePlanCommand>
{
    public DeletePlanCommandValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty()
            .WithMessage("Plan ID is required.");
    }
}

public class DeletePlanCommandHandler(ISuperAdminService superAdminService) : IRequestHandler<DeletePlanCommand, Result<bool>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<bool>> Handle(DeletePlanCommand request, CancellationToken cancellationToken)
    {
        return await _superAdminService.DeletePlanAsync(request.PlanId);
    }
}
