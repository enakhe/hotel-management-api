using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

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

public class DeletePlanCommandHandler(IPlanService service) : IRequestHandler<DeletePlanCommand, Result<bool>>
{
    private readonly IPlanService _service = service;

    public async Task<Result<bool>> Handle(DeletePlanCommand request, CancellationToken cancellationToken)
    {
        return await _service.DeletePlanAsync(request.PlanId);
    }
}
