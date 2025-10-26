using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace HotelManagement.Application.Core.PlanManagement.Commands;

public record BulkUpdatePlansCommand : IRequest<Result<bool>>
{
    public required BulkPlanUpdateRequest[] Updates { get; init; }
}

public record BulkPlanUpdateRequest
{
    public Guid Id { get; init; }
    public required UpdatePlanRequest Data { get; init; }
}

public class BulkUpdatePlansCommandValidator : AbstractValidator<BulkUpdatePlansCommand>
{
    public BulkUpdatePlansCommandValidator()
    {
        RuleFor(x => x.Updates)
            .NotEmpty()
            .WithMessage("At least one plan update is required.");

        RuleFor(x => x.Updates)
            .Must(updates => updates.Length <= 100)
            .WithMessage("Cannot update more than 100 plans at once.");

        RuleForEach(x => x.Updates)
            .SetValidator(new BulkPlanUpdateRequestValidator());
    }
}

public class BulkPlanUpdateRequestValidator : AbstractValidator<BulkPlanUpdateRequest>
{
    public BulkPlanUpdateRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Plan ID is required.");
    }
}

public class BulkUpdatePlansCommandHandler(ISuperAdminService superAdminService) : IRequestHandler<BulkUpdatePlansCommand, Result<bool>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<bool>> Handle(BulkUpdatePlansCommand request, CancellationToken cancellationToken)
    {
        return await _superAdminService.BulkUpdatePlansAsync(request.Updates);
    }
}
