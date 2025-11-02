using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.ReportManagement.Queries;

public record GetReportTypesQuery : IRequest<Result<List<ReportTypeDto>>>
{
}

public class GetReportTypesQueryValidator : AbstractValidator<GetReportTypesQuery>
{
    public GetReportTypesQueryValidator()
    {
        // No validation needed
    }
}

public class GetReportTypesQueryHandler : IRequestHandler<GetReportTypesQuery, Result<List<ReportTypeDto>>>
{
    private readonly IReportService _reportService;

    public GetReportTypesQueryHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<Result<List<ReportTypeDto>>> Handle(GetReportTypesQuery request, CancellationToken cancellationToken)
    {
        return await _reportService.GetReportTypesAsync(cancellationToken);
    }
}

