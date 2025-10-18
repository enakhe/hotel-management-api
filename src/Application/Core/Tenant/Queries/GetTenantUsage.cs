using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Tenant.Queries;

public record GetTenantUsageCommand : IRequest<Result<TenantUsage>>
{
    public Guid TenantId { get; init; }
}

public class GetTenantUsageCommandValidator : AbstractValidator<GetTenantUsageCommand>
{
    public GetTenantUsageCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required.");
    }
}

public class GetTenantUsageCommandHandler(ISuperAdminService superAdminService) : IRequestHandler<GetTenantUsageCommand, Result<TenantUsage>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<TenantUsage>> Handle(GetTenantUsageCommand request, CancellationToken cancellationToken)
    {
        return await _superAdminService.GetTenantUsageAsync(request.TenantId);
    }
}
