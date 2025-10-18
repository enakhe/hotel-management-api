using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Tenant.Commands;

public record UpdateTenantCommand : IRequest<Result<bool>>
{
    public Guid TenantId { get; init; }
    public UpdateTenantRequest Request { get; init; } = new();
}

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(v => v.TenantId).NotEmpty().WithMessage("TenantId is required");
        RuleFor(v => v.Request).NotNull().WithMessage("Request is required");
    }
}

public class UpdateTenantCommandHandler(ISuperAdminService superAdminService) : IRequestHandler<UpdateTenantCommand, Result<bool>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<bool>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        return await _superAdminService.UpdateTenantAsync(request.TenantId, request.Request);
    }
}
