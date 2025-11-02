using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.ReportManagement.Queries;

public record GetReportByIdQuery : IRequest<Result<ReportResponseDto>>
{
    public Guid ReportId { get; init; }
}

public class GetReportByIdQueryValidator : AbstractValidator<GetReportByIdQuery>
{
    public GetReportByIdQueryValidator()
    {
        RuleFor(v => v.ReportId)
            .NotEmpty().WithMessage("Report ID is required");
    }
}

public class GetReportByIdQueryHandler : IRequestHandler<GetReportByIdQuery, Result<ReportResponseDto>>
{
    private readonly IReportService _reportService;

    public GetReportByIdQueryHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<Result<ReportResponseDto>> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        return await _reportService.GetReportByIdAsync(request.ReportId, cancellationToken);
    }
}

