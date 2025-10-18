using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Tenant.Commands;

public record SetTenantModeCommand : IRequest<Result<bool>>
{
    public Guid TenantId { get; init; }
    public TenantMode Mode { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public class SetTenantModeCommandValidator : AbstractValidator<SetTenantModeCommand>
{
    public SetTenantModeCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.Mode)
            .IsInEnum().
            WithMessage("Invalid tenant mode.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required.");
    }
}

public class SetTenantModeCommandHandler(ISuperAdminService superAdminService) : IRequestHandler<SetTenantModeCommand, Result<bool>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<bool>> Handle(SetTenantModeCommand request, CancellationToken cancellationToken)
    {
        return await _superAdminService.SetTenantModeAsync(request.TenantId, request.Mode, request.Reason);
    }
}
