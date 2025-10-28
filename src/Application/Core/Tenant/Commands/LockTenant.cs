using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Tenant.Commands;

public record LockTenantCommand : IRequest<Result<bool>>
{
    public Guid TenantId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public class LockTenantCommandValidator : AbstractValidator<LockTenantCommand>
{
    public LockTenantCommandValidator()
    {
        RuleFor(v => v.TenantId).NotEmpty().WithMessage("TenantId is required");
        RuleFor(v => v.Reason).NotEmpty().WithMessage("Reason is required");
    }
}

public class LockTenantCommandHandler(ITenantService service) : IRequestHandler<LockTenantCommand, Result<bool>>
{
    private readonly ITenantService _service = service;
    public async Task<Result<bool>> Handle(LockTenantCommand request, CancellationToken cancellationToken)
    {
        return await _service.LockTenantAsync(request.TenantId, request.Reason);
    }
}
