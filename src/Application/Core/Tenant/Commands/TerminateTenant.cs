using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Tenant.Commands;

public record TerminateTenantCommand : IRequest<Result<bool>>
{
    public Guid TenantId { get; init; }
    public required TerminateTenantRequest TerminateRequest { get; init; }
}

public class TerminateTenantCommandValidator : AbstractValidator<TerminateTenantCommand>
{
    public TerminateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.TerminateRequest.Reason)
            .NotEmpty()
            .WithMessage("Termination reason is required.");
    }
}

public class TerminateTenantCommandHandler(ISuperAdminService superAdminService) : IRequestHandler<TerminateTenantCommand, Result<bool>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<bool>> Handle(TerminateTenantCommand request, CancellationToken cancellationToken)
    {
        return await _superAdminService.TerminateTenantAsync(request.TenantId, request.TerminateRequest.Reason, request.TerminateRequest.EffectiveDate);
    }
}
