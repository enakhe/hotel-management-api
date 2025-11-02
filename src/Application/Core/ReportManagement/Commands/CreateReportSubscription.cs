using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Application.Core.ReportManagement.Commands;

public record CreateReportSubscriptionCommand : IRequest<Result<ReportSubscriptionDto>>
{
    public Guid SuperAdminId { get; init; }
    public required string Email { get; init; }
    public Guid ReportScheduleId { get; init; }
}

public class CreateReportSubscriptionCommandValidator : AbstractValidator<CreateReportSubscriptionCommand>
{
    public CreateReportSubscriptionCommandValidator()
    {
        RuleFor(v => v.SuperAdminId)
            .NotEmpty().WithMessage("SuperAdmin ID is required");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address");

        RuleFor(v => v.ReportScheduleId)
            .NotEmpty().WithMessage("Report schedule ID is required");
    }
}

public class CreateReportSubscriptionCommandHandler : IRequestHandler<CreateReportSubscriptionCommand, Result<ReportSubscriptionDto>>
{
    private readonly IReportSubscriptionService _subscriptionService;

    public CreateReportSubscriptionCommandHandler(IReportSubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    public async Task<Result<ReportSubscriptionDto>> Handle(CreateReportSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var createDto = new CreateReportSubscriptionDto
        {
            SuperAdminId = request.SuperAdminId,
            Email = request.Email,
            ReportScheduleId = request.ReportScheduleId
        };

        return await _subscriptionService.CreateSubscriptionAsync(createDto, cancellationToken);
    }
}

