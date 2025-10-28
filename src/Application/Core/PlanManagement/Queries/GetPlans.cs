using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.PlanManagement.Queries;

public record GetPlansQuery : IRequest<Result<PaginatedResult<PlanResponseDto>>>
{
    public string? Query { get; init; }
    public bool? IsActive { get; init; }
    public string? BillingCycle { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public string SortBy { get; init; } = "createdAt";
    public bool SortDescending { get; init; } = true;
}

public class GetPlansQueryValidator : AbstractValidator<GetPlansQuery>
{
    public GetPlansQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than zero.");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100)
            .WithMessage("Size must be between 1 and 100.");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .WithMessage("Invalid sort field. Valid fields are: name, createdat, updatedat, isactive, ispopular.");

        RuleFor(x => x.BillingCycle)
            .Must(billingCycle => billingCycle == null || BeValidBillingCycle(billingCycle!))
            .When(x => !string.IsNullOrEmpty(x.BillingCycle))
            .WithMessage("Invalid billing cycle. Valid values are: monthly, yearly, lifetime.");
    }

    private static bool BeValidSortField(string sortBy)
    {
        var validFields = new[] { "name", "createdat", "updatedat", "isactive", "ispopular" };
        return validFields.Contains(sortBy.ToLowerInvariant());
    }

    private static bool BeValidBillingCycle(string billingCycle)
    {
        var validCycles = new[] { "monthly", "yearly", "lifetime" };
        return validCycles.Contains(billingCycle.ToLowerInvariant());
    }
}

public class GetPlansQueryHandler(IPlanService service) : IRequestHandler<GetPlansQuery, Result<PaginatedResult<PlanResponseDto>>>
{
    private readonly IPlanService _service = service;

    public async Task<Result<PaginatedResult<PlanResponseDto>>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var planListRequest = new PlanListRequest
        {
            Query = request.Query,
            IsActive = request.IsActive,
            BillingCycle = request.BillingCycle,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        return await _service.GetPlansAsync(planListRequest);
    }
}
