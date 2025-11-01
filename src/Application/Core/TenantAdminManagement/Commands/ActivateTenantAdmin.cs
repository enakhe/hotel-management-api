using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.TenantAdminManagement.Commands;

/// <summary>
/// Command to activate a Tenant Administrator account
/// </summary>
public record ActivateTenantAdminCommand : IRequest<Result<bool>>
{
    public required Guid UserId { get; init; }
}

public class ActivateTenantAdminCommandValidator : AbstractValidator<ActivateTenantAdminCommand>
{
    public ActivateTenantAdminCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty.");
    }
}

public class ActivateTenantAdminCommandHandler : IRequestHandler<ActivateTenantAdminCommand, Result<bool>>
{
    private readonly ITenantAdminService _tenantAdminService;

    public ActivateTenantAdminCommandHandler(ITenantAdminService tenantAdminService)
    {
        _tenantAdminService = tenantAdminService;
    }

    public async Task<Result<bool>> Handle(ActivateTenantAdminCommand request, CancellationToken cancellationToken)
    {
        return await _tenantAdminService.ActivateAsync(request.UserId);
    }
}

