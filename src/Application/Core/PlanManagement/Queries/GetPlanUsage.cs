using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
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

public class GetPlanUsageQueryHandler(ISuperAdminService superAdminService) : IRequestHandler<GetPlanUsageQuery, Result<PlanUsageDto[]>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<PlanUsageDto[]>> Handle(GetPlanUsageQuery request, CancellationToken cancellationToken)
    {
        return await _superAdminService.GetPlanUsageAsync();
    }
}
