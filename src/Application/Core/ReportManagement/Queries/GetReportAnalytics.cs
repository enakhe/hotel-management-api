using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.ReportManagement.Queries;

public record GetReportAnalyticsQuery : IRequest<Result<ReportAnalyticsDto>>
{
}

public class GetReportAnalyticsQueryValidator : AbstractValidator<GetReportAnalyticsQuery>
{
    public GetReportAnalyticsQueryValidator()
    {
        // No validation needed
    }
}

public class GetReportAnalyticsQueryHandler(IReportService reportService) : IRequestHandler<GetReportAnalyticsQuery, Result<ReportAnalyticsDto>>
{
    private readonly IReportService _reportService = reportService;

    public async Task<Result<ReportAnalyticsDto>> Handle(GetReportAnalyticsQuery request, CancellationToken cancellationToken)
    {
        return await _reportService.GetReportAnalyticsAsync(cancellationToken);
    }
}

