using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Tenant.Commands;

public record UnlockTenantCommand : IRequest<Result<bool>>
{
    public Guid TenantId { get; init; }
    public required UnlockTenantRequest Request { get; init; }
}

public class UnlockTenantCommandValidator : AbstractValidator<UnlockTenantCommand>
{
    public UnlockTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required.");
        RuleFor(x => x.Request).NotNull().WithMessage("Unlock tenant request is required.");
        RuleFor(x => x.Request.Reason).NotEmpty().WithMessage("Reason for unlocking the tenant is required.");
    }
}

public class UnlockTenantCommandHandler(ISuperAdminService superAdminService) : IRequestHandler<UnlockTenantCommand, Result<bool>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<bool>> Handle(UnlockTenantCommand request, CancellationToken cancellationToken)
    {
        return await _superAdminService.UnlockTenantAsync(request.TenantId, request.Request.Reason);
    }
}
