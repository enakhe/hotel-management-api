using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Core.ReportManagement.Queries;

public record GetReportsQuery : IRequest<Result<PaginatedResult<ReportResponseDto>>>
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public ReportType? Type { get; init; }
    public ReportStatus? Status { get; init; }
    public ReportFormat? Format { get; init; }
    public Guid? GeneratedBy { get; init; }
    public Guid? TenantId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}

public class GetReportsQueryValidator : AbstractValidator<GetReportsQuery>
{
    public GetReportsQueryValidator()
    {
        RuleFor(v => v.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(v => v.Size)
            .GreaterThan(0).WithMessage("Size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Size must not exceed 100");
    }
}

public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, Result<PaginatedResult<ReportResponseDto>>>
{
    private readonly IReportService _reportService;

    public GetReportsQueryHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<Result<PaginatedResult<ReportResponseDto>>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var filter = new ReportFilterDto
        {
            Page = request.Page,
            Size = request.Size,
            Type = request.Type,
            Status = request.Status,
            Format = request.Format,
            GeneratedBy = request.GeneratedBy,
            TenantId = request.TenantId,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            SearchTerm = request.SearchTerm,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        return await _reportService.GetReportsAsync(filter, cancellationToken);
    }
}

