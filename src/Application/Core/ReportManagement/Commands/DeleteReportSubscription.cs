using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Application.Core.ReportManagement.Commands;

public record DeleteReportSubscriptionCommand : IRequest<Result<bool>>
{
    public Guid SubscriptionId { get; init; }
}

public class DeleteReportSubscriptionCommandValidator : AbstractValidator<DeleteReportSubscriptionCommand>
{
    public DeleteReportSubscriptionCommandValidator()
    {
        RuleFor(v => v.SubscriptionId)
            .NotEmpty().WithMessage("Subscription ID is required");
    }
}

public class DeleteReportSubscriptionCommandHandler : IRequestHandler<DeleteReportSubscriptionCommand, Result<bool>>
{
    private readonly IReportSubscriptionService _subscriptionService;

    public DeleteReportSubscriptionCommandHandler(IReportSubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    public async Task<Result<bool>> Handle(DeleteReportSubscriptionCommand request, CancellationToken cancellationToken)
    {
        return await _subscriptionService.DeleteSubscriptionAsync(request.SubscriptionId, cancellationToken);
    }
}

