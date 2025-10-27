using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.Tenant;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Tenant.Commands;

public record PurgeTenantDataCommand : IRequest<Result<bool>>
{
    public Guid TenantId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public class PurgeTenantDataCommandValidator : AbstractValidator<PurgeTenantDataCommand>
{
    public PurgeTenantDataCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant Id is required");
        RuleFor(x => x.Reason).NotEmpty()
            .WithMessage("Reason for data purge is required")
            .MaximumLength(500)
            .WithMessage("Reason cannot exceed 500 characters");
    }
}

public class PurgeTenantDataCommandHandler(ITenantService service) : IRequestHandler<PurgeTenantDataCommand, Result<bool>>
{
    private readonly ITenantService _service = service;

    public async Task<Result<bool>> Handle(PurgeTenantDataCommand request, CancellationToken cancellationToken)
    {
        return await _service.PurgeTenantDataAsync(request.TenantId, request.Reason);
    }
}
