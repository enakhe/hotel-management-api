using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Common.DTOs.Tenant;
using HotelManagement.Domain.Common;
using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.Tenant;

namespace HotelManagement.Application.Tenant.Queries.GetTenants;

public record GetTenantsQuery : IRequest<Result<PaginatedResult<TenantSummary>>>
{
    public string? Query { get; init; }
    public bool? Status { get; init; }
    public string? Plan { get; init; }
    public string? Region { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
    public string? SortBy { get; init; } = "CreatedAt";
    public bool SortDescending { get; init; } = true;
}

public class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
    public GetTenantsQueryValidator()
    {
        
    }
}

public class GetTenantsQueryHandler(ITenantService service) : IRequestHandler<GetTenantsQuery, Result<PaginatedResult<TenantSummary>>>
{
    private readonly ITenantService _service = service;

    public async Task<Result<PaginatedResult<TenantSummary>>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenantListRequest = new TenantListRequest
        {
            Query = request.Query,
            Status = request.Status,
            Plan = request.Plan,
            Region = request.Region,
            CreatedFrom = request.CreatedFrom,
            CreatedTo = request.CreatedTo,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        return await _service.GetTenantsAsync(tenantListRequest);
    }
}
