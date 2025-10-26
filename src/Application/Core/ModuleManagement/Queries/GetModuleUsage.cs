using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace HotelManagement.Application.Core.ModuleManagement.Queries;

public record GetModuleUsageQuery : IRequest<Result<ModuleUsageDto[]>>
{
}

public class GetModuleUsageQueryValidator : AbstractValidator<GetModuleUsageQuery>
{
    public GetModuleUsageQueryValidator()
    {
        // No validation needed for analytics queries
    }
}

public class GetModuleUsageQueryHandler(ISuperAdminService superAdminService) : IRequestHandler<GetModuleUsageQuery, Result<ModuleUsageDto[]>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<ModuleUsageDto[]>> Handle(GetModuleUsageQuery request, CancellationToken cancellationToken)
    {
        return await _superAdminService.GetModuleUsageAsync();
    }
}
