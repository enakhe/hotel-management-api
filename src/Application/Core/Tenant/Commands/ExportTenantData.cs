using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Tenant.Commands;

public record ExportTenantDataCommand : IRequest<Result<ExportJobResult>>
{
    public Guid TenantId { get; init; }
    public bool IncludeUsers { get; init; } = true;
    public bool IncludeReservations { get; init; } = true;
    public bool IncludeRooms { get; init; } = true;
    public bool IncludeAuditLogs { get; init; } = false;
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string Format { get; init; } = "JSON";
}

public class ExportTenantDataCommandValidator : AbstractValidator<ExportTenantDataCommand>
{
    public ExportTenantDataCommandValidator()
    {
        RuleFor(v => v.TenantId).NotEmpty().WithMessage("TenantId is required");

        RuleFor(v => v.Format)
            .Must(format => format == "JSON" || format == "CSV" || format == "Excel")
            .WithMessage("Format must be one of the following: JSON, CSV, Excel");

        RuleFor(v => v.FromDate)
            .LessThanOrEqualTo(v => v.ToDate)
            .When(v => v.FromDate.HasValue && v.ToDate.HasValue)
            .WithMessage("FromDate must be less than or equal to ToDate");

        RuleFor(v => v.ToDate)
            .GreaterThanOrEqualTo(v => v.FromDate)
            .When(v => v.FromDate.HasValue && v.ToDate.HasValue)
            .WithMessage("ToDate must be greater than or equal to FromDate");
    }
}

public class ExportTenantDataCommandHandler(ITenantService service) : IRequestHandler<ExportTenantDataCommand, Result<ExportJobResult>>
{
    private readonly ITenantService _service = service;

    public async Task<Result<ExportJobResult>> Handle(ExportTenantDataCommand request, CancellationToken cancellationToken)
    {
        var exportOptions = new ExportOptions
        {
            IncludeUsers = request.IncludeUsers,
            IncludeReservations = request.IncludeReservations,
            IncludeRooms = request.IncludeRooms,
            IncludeAuditLogs = request.IncludeAuditLogs,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Format = request.Format
        };

        return await _service.ExportTenantDataAsync(request.TenantId, exportOptions);
    }
}
