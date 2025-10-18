using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Tenant.Commands;

public record ExportTenantDataCommand : IRequest<Result<ExportJobResult>>
{
    public Guid TenantId { get; init; }
    public ExportOptions Request { get; init; } = new();
}

public class ExportTenantDataCommandValidator : AbstractValidator<ExportTenantDataCommand>
{
    public ExportTenantDataCommandValidator()
    {
        RuleFor(v => v.TenantId).NotEmpty().WithMessage("TenantId is required");
        RuleFor(v => v.Request).NotNull().WithMessage("Request is required");
    }
}

public class ExportTenantDataCommandHandler(ISuperAdminService superAdminService) : IRequestHandler<ExportTenantDataCommand, Result<ExportJobResult>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<ExportJobResult>> Handle(ExportTenantDataCommand request, CancellationToken cancellationToken)
    {
        return await _superAdminService.ExportTenantDataAsync(request.TenantId, request.Request);
    }
}
