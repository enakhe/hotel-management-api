using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.ReportManagement.Commands;

public record DeleteReportCommand : IRequest<Result<bool>>
{
    public Guid ReportId { get; init; }
}

public class DeleteReportCommandValidator : AbstractValidator<DeleteReportCommand>
{
    public DeleteReportCommandValidator()
    {
        RuleFor(v => v.ReportId)
            .NotEmpty().WithMessage("Report ID is required");
    }
}

public class DeleteReportCommandHandler : IRequestHandler<DeleteReportCommand, Result<bool>>
{
    private readonly IReportService _reportService;

    public DeleteReportCommandHandler(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<Result<bool>> Handle(DeleteReportCommand request, CancellationToken cancellationToken)
    {
        return await _reportService.DeleteReportAsync(request.ReportId, cancellationToken);
    }
}

