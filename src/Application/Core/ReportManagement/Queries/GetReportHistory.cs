using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Core.ReportManagement.Queries;

public record GetReportHistoryQuery : IRequest<Result<PaginatedResult<ReportExecutionDto>>>
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public Guid? ReportScheduleId { get; init; }
    public Guid? InitiatedBy { get; init; }
    public ReportStatus? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public bool ScheduledOnly { get; init; } = false;
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}

public class GetReportHistoryQueryValidator : AbstractValidator<GetReportHistoryQuery>
{
    public GetReportHistoryQueryValidator()
    {
        RuleFor(v => v.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(v => v.Size)
            .GreaterThan(0).WithMessage("Size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Size must not exceed 100");
    }
}

public class GetReportHistoryQueryHandler : IRequestHandler<GetReportHistoryQuery, Result<PaginatedResult<ReportExecutionDto>>>
{
    private readonly IReportService _reportService;

    public GetReportHistoryQueryHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<Result<PaginatedResult<ReportExecutionDto>>> Handle(GetReportHistoryQuery request, CancellationToken cancellationToken)
    {
        var filter = new ReportHistoryFilterDto
        {
            Page = request.Page,
            Size = request.Size,
            ReportScheduleId = request.ReportScheduleId,
            InitiatedBy = request.InitiatedBy,
            Status = request.Status,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            ScheduledOnly = request.ScheduledOnly,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        return await _reportService.GetReportHistoryAsync(filter, cancellationToken);
    }
}

