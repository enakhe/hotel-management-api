using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Core.ReportManagement.Commands;

public record UpdateReportScheduleCommand : IRequest<Result<ReportScheduleDto>>
{
    public Guid ScheduleId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public ReportFormat? Format { get; init; }
    public ScheduleFrequency? Frequency { get; init; }
    public string? CronExpression { get; init; }
    public ReportParametersDto? Parameters { get; init; }
    public Dictionary<string, object>? Filters { get; init; }
    public List<string>? EmailRecipients { get; init; }
    public string? TimeZone { get; init; }
}

public class UpdateReportScheduleCommandValidator : AbstractValidator<UpdateReportScheduleCommand>
{
    public UpdateReportScheduleCommandValidator()
    {
        RuleFor(v => v.ScheduleId)
            .NotEmpty().WithMessage("Schedule ID is required");
    }
}

public class UpdateReportScheduleCommandHandler(IReportSchedulerService schedulerService) : IRequestHandler<UpdateReportScheduleCommand, Result<ReportScheduleDto>>
{
    private readonly IReportSchedulerService _schedulerService = schedulerService;

    public async Task<Result<ReportScheduleDto>> Handle(UpdateReportScheduleCommand request, CancellationToken cancellationToken)
    {
        var updateDto = new UpdateReportScheduleDto
        {
            Name = request.Name,
            Description = request.Description,
            Format = request.Format,
            Frequency = request.Frequency,
            CronExpression = request.CronExpression,
            Parameters = request.Parameters,
            Filters = request.Filters,
            EmailRecipients = request.EmailRecipients,
            TimeZone = request.TimeZone
        };

        return await _schedulerService.UpdateScheduleAsync(request.ScheduleId, updateDto, cancellationToken);
    }
}

