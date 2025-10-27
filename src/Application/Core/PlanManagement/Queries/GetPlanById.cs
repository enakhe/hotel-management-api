using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.Services;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace HotelManagement.Application.Core.PlanManagement.Queries;

public record GetPlanByIdQuery : IRequest<Result<PlanResponseDto>>
{
    public Guid PlanId { get; init; }
}

public class GetPlanByIdQueryValidator : AbstractValidator<GetPlanByIdQuery>
{
    public GetPlanByIdQueryValidator()
    {
        RuleFor(x => x.PlanId)
            .NotEmpty()
            .WithMessage("Plan ID is required.");
    }
}

public class GetPlanByIdQueryHandler(IPlanService service) : IRequestHandler<GetPlanByIdQuery, Result<PlanResponseDto>>
{
    private readonly IPlanService _service = service;

    public async Task<Result<PlanResponseDto>> Handle(GetPlanByIdQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetPlanByIdAsync(request.PlanId);
    }
}
