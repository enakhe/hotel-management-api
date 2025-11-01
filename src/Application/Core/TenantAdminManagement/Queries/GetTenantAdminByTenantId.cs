using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.TenantAdminManagement.Queries;

/// <summary>
/// Query to get the Administrator for a specific tenant
/// </summary>
public record GetTenantAdminByTenantIdQuery : IRequest<Result<TenantAdminDto>>
{
    public required Guid TenantId { get; init; }
}

public class GetTenantAdminByTenantIdQueryValidator : AbstractValidator<GetTenantAdminByTenantIdQuery>
{
    public GetTenantAdminByTenantIdQueryValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Tenant ID cannot be empty.");
    }
}

public class GetTenantAdminByTenantIdQueryHandler : IRequestHandler<GetTenantAdminByTenantIdQuery, Result<TenantAdminDto>>
{
    private readonly ITenantAdminService _tenantAdminService;

    public GetTenantAdminByTenantIdQueryHandler(ITenantAdminService tenantAdminService)
    {
        _tenantAdminService = tenantAdminService;
    }

    public async Task<Result<TenantAdminDto>> Handle(GetTenantAdminByTenantIdQuery request, CancellationToken cancellationToken)
    {
        return await _tenantAdminService.GetTenantAdminByTenantIdAsync(request.TenantId);
    }
}

