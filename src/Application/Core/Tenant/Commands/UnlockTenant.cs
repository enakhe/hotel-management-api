using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.Tenant;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Tenant.Commands;

public record UnlockTenantCommand : IRequest<Result<bool>>
{
    public Guid TenantId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public class UnlockTenantCommandValidator : AbstractValidator<UnlockTenantCommand>
{
    public UnlockTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant ID is required.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason for unlocking the tenant is required.");
    }
}

public class UnlockTenantCommandHandler(ITenantService service) : IRequestHandler<UnlockTenantCommand, Result<bool>>
{
    private readonly ITenantService _service = service;

    public async Task<Result<bool>> Handle(UnlockTenantCommand request, CancellationToken cancellationToken)
    {
        return await _service.UnlockTenantAsync(request.TenantId, request.Reason);
    }
}
