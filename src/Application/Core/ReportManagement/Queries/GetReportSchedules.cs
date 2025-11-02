using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Core.ReportManagement.Queries;

public record GetReportSchedulesQuery : IRequest<Result<PaginatedResult<ReportScheduleDto>>>
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public ReportType? ReportType { get; init; }
    public bool? IsActive { get; init; }
    public Guid? CreatedBy { get; init; }
    public Guid? TenantId { get; init; }
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}

public class GetReportSchedulesQueryValidator : AbstractValidator<GetReportSchedulesQuery>
{
    public GetReportSchedulesQueryValidator()
    {
        RuleFor(v => v.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(v => v.Size)
            .GreaterThan(0).WithMessage("Size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Size must not exceed 100");
    }
}

public class GetReportSchedulesQueryHandler : IRequestHandler<GetReportSchedulesQuery, Result<PaginatedResult<ReportScheduleDto>>>
{
    private readonly IReportSchedulerService _schedulerService;

    public GetReportSchedulesQueryHandler(IReportSchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    public async Task<Result<PaginatedResult<ReportScheduleDto>>> Handle(GetReportSchedulesQuery request, CancellationToken cancellationToken)
    {
        var filter = new ScheduleFilterDto
        {
            Page = request.Page,
            Size = request.Size,
            ReportType = request.ReportType,
            IsActive = request.IsActive,
            CreatedBy = request.CreatedBy,
            TenantId = request.TenantId,
            SearchTerm = request.SearchTerm,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        return await _schedulerService.GetSchedulesAsync(filter, cancellationToken);
    }
}

