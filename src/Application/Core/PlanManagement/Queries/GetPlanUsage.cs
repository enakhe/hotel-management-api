using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.Services;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace HotelManagement.Application.Core.PlanManagement.Queries;

public record GetPlanUsageQuery : IRequest<Result<PlanUsageDto[]>>
{
}

public class GetPlanUsageQueryValidator : AbstractValidator<GetPlanUsageQuery>
{
    public GetPlanUsageQueryValidator()
    {
        // No validation needed for analytics queries
    }
}

public class GetPlanUsageQueryHandler(IPlanService service) : IRequestHandler<GetPlanUsageQuery, Result<PlanUsageDto[]>>
{
    private readonly IPlanService _service = service;

    public async Task<Result<PlanUsageDto[]>> Handle(GetPlanUsageQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetPlanUsageAsync();
    }
}
