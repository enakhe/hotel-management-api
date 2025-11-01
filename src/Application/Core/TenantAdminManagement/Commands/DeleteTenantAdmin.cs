using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.TenantAdminManagement.Commands;

/// <summary>
/// Command to delete a Tenant Administrator
/// </summary>
public record DeleteTenantAdminCommand : IRequest<Result<bool>>
{
    public required Guid UserId { get; init; }
}

public class DeleteTenantAdminCommandValidator : AbstractValidator<DeleteTenantAdminCommand>
{
    public DeleteTenantAdminCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty.");
    }
}

public class DeleteTenantAdminCommandHandler : IRequestHandler<DeleteTenantAdminCommand, Result<bool>>
{
    private readonly ITenantAdminService _tenantAdminService;

    public DeleteTenantAdminCommandHandler(ITenantAdminService tenantAdminService)
    {
        _tenantAdminService = tenantAdminService;
    }

    public async Task<Result<bool>> Handle(DeleteTenantAdminCommand request, CancellationToken cancellationToken)
    {
        return await _tenantAdminService.DeleteAsync(request.UserId);
    }
}

