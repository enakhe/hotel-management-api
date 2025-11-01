using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.TenantAdminManagement.Commands;

/// <summary>
/// Command to deactivate a Tenant Administrator account
/// </summary>
public record DeactivateTenantAdminCommand : IRequest<Result<bool>>
{
    public required Guid UserId { get; init; }
}

public class DeactivateTenantAdminCommandValidator : AbstractValidator<DeactivateTenantAdminCommand>
{
    public DeactivateTenantAdminCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty.");
    }
}

public class DeactivateTenantAdminCommandHandler : IRequestHandler<DeactivateTenantAdminCommand, Result<bool>>
{
    private readonly ITenantAdminService _tenantAdminService;

    public DeactivateTenantAdminCommandHandler(ITenantAdminService tenantAdminService)
    {
        _tenantAdminService = tenantAdminService;
    }

    public async Task<Result<bool>> Handle(DeactivateTenantAdminCommand request, CancellationToken cancellationToken)
    {
        return await _tenantAdminService.DeactivateAsync(request.UserId);
    }
}

