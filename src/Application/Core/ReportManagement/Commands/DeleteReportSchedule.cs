using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.ReportManagement.Commands;

public record DeleteReportScheduleCommand : IRequest<Result<bool>>
{
    public Guid ScheduleId { get; init; }
}

public class DeleteReportScheduleCommandValidator : AbstractValidator<DeleteReportScheduleCommand>
{
    public DeleteReportScheduleCommandValidator()
    {
        RuleFor(v => v.ScheduleId)
            .NotEmpty().WithMessage("Schedule ID is required");
    }
}

public class DeleteReportScheduleCommandHandler : IRequestHandler<DeleteReportScheduleCommand, Result<bool>>
{
    private readonly IReportSchedulerService _schedulerService;

    public DeleteReportScheduleCommandHandler(IReportSchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    public async Task<Result<bool>> Handle(DeleteReportScheduleCommand request, CancellationToken cancellationToken)
    {
        return await _schedulerService.DeleteScheduleAsync(request.ScheduleId, cancellationToken);
    }
}

