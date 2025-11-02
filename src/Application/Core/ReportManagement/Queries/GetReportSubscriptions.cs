using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Application.Core.ReportManagement.Queries;

public record GetReportSubscriptionsQuery : IRequest<Result<List<ReportSubscriptionDto>>>
{
    public Guid? ReportScheduleId { get; init; }
    public Guid? SuperAdminId { get; init; }
    public bool? IsActive { get; init; }
}

public class GetReportSubscriptionsQueryValidator : AbstractValidator<GetReportSubscriptionsQuery>
{
    public GetReportSubscriptionsQueryValidator()
    {
        // No strict validation needed
    }
}

public class GetReportSubscriptionsQueryHandler : IRequestHandler<GetReportSubscriptionsQuery, Result<List<ReportSubscriptionDto>>>
{
    private readonly IReportSubscriptionService _subscriptionService;

    public GetReportSubscriptionsQueryHandler(IReportSubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    public async Task<Result<List<ReportSubscriptionDto>>> Handle(GetReportSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        return await _subscriptionService.GetSubscriptionsAsync(
            request.ReportScheduleId,
            request.SuperAdminId,
            request.IsActive,
            cancellationToken);
    }
}

