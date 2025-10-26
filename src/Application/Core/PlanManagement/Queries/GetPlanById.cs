using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
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

public class GetPlanByIdQueryHandler(ISuperAdminService superAdminService) : IRequestHandler<GetPlanByIdQuery, Result<PlanResponseDto>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<PlanResponseDto>> Handle(GetPlanByIdQuery request, CancellationToken cancellationToken)
    {
        return await _superAdminService.GetPlanByIdAsync(request.PlanId);
    }
}
