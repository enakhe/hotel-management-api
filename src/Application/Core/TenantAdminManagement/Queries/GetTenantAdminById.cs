using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.TenantAdminManagement.Queries;

/// <summary>
/// Query to get a Tenant Administrator by ID
/// </summary>
public record GetTenantAdminByIdQuery : IRequest<Result<TenantAdminDto>>
{
    public required Guid UserId { get; init; }
}

public class GetTenantAdminByIdQueryValidator : AbstractValidator<GetTenantAdminByIdQuery>
{
    public GetTenantAdminByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty.");
    }
}

public class GetTenantAdminByIdQueryHandler : IRequestHandler<GetTenantAdminByIdQuery, Result<TenantAdminDto>>
{
    private readonly ITenantAdminService _tenantAdminService;

    public GetTenantAdminByIdQueryHandler(ITenantAdminService tenantAdminService)
    {
        _tenantAdminService = tenantAdminService;
    }

    public async Task<Result<TenantAdminDto>> Handle(GetTenantAdminByIdQuery request, CancellationToken cancellationToken)
    {
        return await _tenantAdminService.GetTenantAdminByIdAsync(request.UserId);
    }
}

