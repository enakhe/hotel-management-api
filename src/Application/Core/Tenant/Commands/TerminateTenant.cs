using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.Tenant;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Tenant.Commands;

public record TerminateTenantCommand : IRequest<Result<bool>>
{
    public Guid TenantId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime? EffectiveDate { get; init; } = DateTime.UtcNow;
}

public class TerminateTenantCommandValidator : AbstractValidator<TerminateTenantCommand>
{
    public TerminateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Tenant ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Termination reason is required.");

        RuleFor(x => x.EffectiveDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Effective date must be in the future or today.");
    }
}

public class TerminateTenantCommandHandler(ITenantService service) : IRequestHandler<TerminateTenantCommand, Result<bool>>
{
    private readonly ITenantService _service = service;

    public async Task<Result<bool>> Handle(TerminateTenantCommand request, CancellationToken cancellationToken)
    {
        return await _service.TerminateTenantAsync(request.TenantId, request.Reason, request.EffectiveDate);
    }
}
