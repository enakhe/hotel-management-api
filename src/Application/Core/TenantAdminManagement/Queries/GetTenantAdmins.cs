using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.TenantAdminManagement.Queries;

/// <summary>
/// Query to get all Tenant Administrators with pagination and filtering
/// </summary>
public record GetTenantAdminsQuery : IRequest<Result<PaginatedResult<TenantAdminDto>>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public Guid? TenantId { get; init; }
    public string? Email { get; init; }
    public bool? IsActive { get; init; }
}

public class GetTenantAdminsQueryValidator : AbstractValidator<GetTenantAdminsQuery>
{
    public GetTenantAdminsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format.");
    }
}

public class GetTenantAdminsQueryHandler : IRequestHandler<GetTenantAdminsQuery, Result<PaginatedResult<TenantAdminDto>>>
{
    private readonly ITenantAdminService _tenantAdminService;

    public GetTenantAdminsQueryHandler(ITenantAdminService tenantAdminService)
    {
        _tenantAdminService = tenantAdminService;
    }

    public async Task<Result<PaginatedResult<TenantAdminDto>>> Handle(GetTenantAdminsQuery request, CancellationToken cancellationToken)
    {
        return await _tenantAdminService.GetTenantAdminsAsync(
            request.Page,
            request.PageSize,
            request.TenantId,
            request.Email,
            request.IsActive);
    }
}

