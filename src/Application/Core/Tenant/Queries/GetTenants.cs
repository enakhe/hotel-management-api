using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Common.DTOs.Tenant;
using HotelManagement.Domain.Common;
using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;

namespace HotelManagement.Application.Tenant.Queries.GetTenants;

public record GetTenantsQuery : IRequest<Result<PaginatedResult<TenantSummary>>>
{
    public TenantListRequest Request { get; init; } = new();
}

public class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
    public GetTenantsQueryValidator()
    {
        RuleFor(v => v.Request).NotNull().WithMessage("Request is required");
        RuleFor(v => v.Request.Page).GreaterThan(0).WithMessage("Page must be greater than 0");
        RuleFor(v => v.Request.Size).GreaterThan(0).WithMessage("Size must be greater than 0");
        RuleFor(v => v.Request.SortBy).NotEmpty().WithMessage("SortBy is required");
        RuleFor(v => v.Request.SortDescending).NotNull().WithMessage("SortDescending is required");
        RuleFor(v => v.Request.Query).NotEmpty().WithMessage("Query is required");
        RuleFor(v => v.Request.Status).NotEmpty().WithMessage("Status is required");
        RuleFor(v => v.Request.Plan).NotEmpty().WithMessage("Plan is required");
        RuleFor(v => v.Request.Region).NotEmpty().WithMessage("Region is required");
        RuleFor(v => v.Request.CreatedFrom).NotEmpty().WithMessage("CreatedFrom is required");
        RuleFor(v => v.Request.CreatedTo).NotEmpty().WithMessage("CreatedTo is required");
    }
}

public class GetTenantsQueryHandler(ISuperAdminService superAdminService) : IRequestHandler<GetTenantsQuery, Result<PaginatedResult<TenantSummary>>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<PaginatedResult<TenantSummary>>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        return await _superAdminService.GetTenantsAsync(request.Request);
    }
}
