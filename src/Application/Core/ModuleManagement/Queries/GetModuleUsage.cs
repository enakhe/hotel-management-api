using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

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

public class GetModuleUsageQueryHandler(IModuleService service) : IRequestHandler<GetModuleUsageQuery, Result<ModuleUsageDto[]>>
{
    private readonly IModuleService _service = service;

    public async Task<Result<ModuleUsageDto[]>> Handle(GetModuleUsageQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetModuleUsageAsync();
    }
}
