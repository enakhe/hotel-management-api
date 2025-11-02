using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Core.ReportManagement.Commands;

public record CreateReportScheduleCommand : IRequest<Result<ReportScheduleDto>>
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public ReportType ReportType { get; init; }
    public ReportFormat Format { get; init; }
    public ScheduleFrequency Frequency { get; init; }
    public string? CronExpression { get; init; }
    public ReportParametersDto? Parameters { get; init; }
    public Dictionary<string, object>? Filters { get; init; }
    public List<string>? EmailRecipients { get; init; }
    public Guid? TenantId { get; init; }
    public string TimeZone { get; init; } = "UTC";
}

public class CreateReportScheduleCommandValidator : AbstractValidator<CreateReportScheduleCommand>
{
    public CreateReportScheduleCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Schedule name is required")
            .MaximumLength(200).WithMessage("Schedule name must not exceed 200 characters");

        RuleFor(v => v.ReportType)
            .IsInEnum().WithMessage("Invalid report type");

        RuleFor(v => v.Format)
            .IsInEnum().WithMessage("Invalid report format");

        RuleFor(v => v.Frequency)
            .IsInEnum().WithMessage("Invalid schedule frequency");
    }
}

public class CreateReportScheduleCommandHandler : IRequestHandler<CreateReportScheduleCommand, Result<ReportScheduleDto>>
{
    private readonly IReportSchedulerService _schedulerService;

    public CreateReportScheduleCommandHandler(IReportSchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    public async Task<Result<ReportScheduleDto>> Handle(CreateReportScheduleCommand request, CancellationToken cancellationToken)
    {
        var createDto = new CreateReportScheduleDto
        {
            Name = request.Name,
            Description = request.Description,
            ReportType = request.ReportType,
            Format = request.Format,
            Frequency = request.Frequency,
            CronExpression = request.CronExpression,
            Parameters = request.Parameters,
            Filters = request.Filters,
            EmailRecipients = request.EmailRecipients,
            TenantId = request.TenantId,
            TimeZone = request.TimeZone
        };

        return await _schedulerService.CreateScheduleAsync(createDto, cancellationToken);
    }
}

