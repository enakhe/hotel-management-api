using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Common.DTOs.Tenant;
using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;

namespace HotelManagement.Application.Core.Tenant.Queries;

public record GetTenantByIdQuery : IRequest<Result<TenantDetail>>
{
    public Guid TenantId { get; init; }
}

public class GetTenantByIdQueryValidator : AbstractValidator<GetTenantByIdQuery>
{
    public GetTenantByIdQueryValidator()
    {
        RuleFor(v => v.TenantId).NotEmpty().WithMessage("TenantId is required");
    }
}

public class GetTenantByIdQueryHandler(ISuperAdminService superAdminService) : IRequestHandler<GetTenantByIdQuery, Result<TenantDetail>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<TenantDetail>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        return await _superAdminService.GetTenantDetailAsync(request.TenantId);
    }
}
