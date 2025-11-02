using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.ReportManagement.Queries;

public record GetReportScheduleByIdQuery : IRequest<Result<ReportScheduleDto>>
{
    public Guid ScheduleId { get; init; }
}

public class GetReportScheduleByIdQueryValidator : AbstractValidator<GetReportScheduleByIdQuery>
{
    public GetReportScheduleByIdQueryValidator()
    {
        RuleFor(v => v.ScheduleId)
            .NotEmpty().WithMessage("Schedule ID is required");
    }
}

public class GetReportScheduleByIdQueryHandler : IRequestHandler<GetReportScheduleByIdQuery, Result<ReportScheduleDto>>
{
    private readonly IReportSchedulerService _schedulerService;

    public GetReportScheduleByIdQueryHandler(IReportSchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    public async Task<Result<ReportScheduleDto>> Handle(GetReportScheduleByIdQuery request, CancellationToken cancellationToken)
    {
        return await _schedulerService.GetScheduleByIdAsync(request.ScheduleId, cancellationToken);
    }
}

