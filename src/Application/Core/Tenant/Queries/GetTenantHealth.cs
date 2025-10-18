using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;

namespace HotelManagement.Application.Core.Tenant.Queries;

public record GetTenantHealthCommand : IRequest<Result<TenantHealth>>
{
    public Guid TenantId { get; init; }
}

public class GetTenantHealthCommandValidator : AbstractValidator<GetTenantHealthCommand>
{
    public GetTenantHealthCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId is required.");
    }
}

public class GetTenantHealthCommandHandler(ISuperAdminService superAdminService) : IRequestHandler<GetTenantHealthCommand, Result<TenantHealth>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<TenantHealth>> Handle(GetTenantHealthCommand request, CancellationToken cancellationToken)
    {
        return await _superAdminService.GetTenantHealthAsync(request.TenantId);
    }
}
