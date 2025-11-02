using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Core.ReportManagement.Commands;

public record GenerateReportCommand : IRequest<Result<ReportResponseDto>>
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public ReportType Type { get; init; }
    public ReportCategory Category { get; init; }
    public ReportFormat Format { get; init; }
    public Guid? TenantId { get; init; }
    public ReportParametersDto? Parameters { get; init; }
    public Dictionary<string, object>? Filters { get; init; }
}

public class GenerateReportCommandValidator : AbstractValidator<GenerateReportCommand>
{
    public GenerateReportCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Report name is required")
            .MaximumLength(200).WithMessage("Report name must not exceed 200 characters");

        RuleFor(v => v.Type)
            .IsInEnum().WithMessage("Invalid report type");

        RuleFor(v => v.Format)
            .IsInEnum().WithMessage("Invalid report format");

        RuleFor(v => v.Category)
            .IsInEnum().WithMessage("Invalid report category");
    }
}

public class GenerateReportCommandHandler : IRequestHandler<GenerateReportCommand, Result<ReportResponseDto>>
{
    private readonly IReportService _reportService;

    public GenerateReportCommandHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<Result<ReportResponseDto>> Handle(GenerateReportCommand request, CancellationToken cancellationToken)
    {
        var reportRequest = new ReportRequestDto
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Category = request.Category,
            Format = request.Format,
            TenantId = request.TenantId,
            Parameters = request.Parameters,
            Filters = request.Filters
        };

        return await _reportService.GenerateReportAsync(reportRequest, cancellationToken);
    }
}

