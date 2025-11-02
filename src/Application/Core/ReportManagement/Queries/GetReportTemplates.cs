using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HotelManagement.Application.Core.ReportManagement.Queries;

public record GetReportTemplatesQuery : IRequest<Result<List<ReportTemplateDto>>>
{
    public ReportType? ReportType { get; init; }
    public bool? IsPublic { get; init; }
    public bool? IsActive { get; init; }
    public Guid? CreatedBy { get; init; }
}

public class GetReportTemplatesQueryValidator : AbstractValidator<GetReportTemplatesQuery>
{
    public GetReportTemplatesQueryValidator()
    {
        // No strict validation needed
    }
}

public class GetReportTemplatesQueryHandler : IRequestHandler<GetReportTemplatesQuery, Result<List<ReportTemplateDto>>>
{
    private readonly IReportTemplateService _templateService;

    public GetReportTemplatesQueryHandler(IReportTemplateService templateService)
    {
        _templateService = templateService;
    }

    public async Task<Result<List<ReportTemplateDto>>> Handle(GetReportTemplatesQuery request, CancellationToken cancellationToken)
    {
        return await _templateService.GetTemplatesAsync(
            request.ReportType,
            request.IsPublic,
            request.IsActive,
            request.CreatedBy,
            cancellationToken);
    }
}

