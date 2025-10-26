using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace HotelManagement.Application.Core.PlanManagement.Queries;

public record GetPlansQuery : IRequest<Result<PaginatedResult<PlanResponseDto>>>
{
    public string? Query { get; init; }
    public bool? IsActive { get; init; }
    public string? BillingCycle { get; init; }
    public decimal? PriceMin { get; init; }
    public decimal? PriceMax { get; init; }
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

        RuleFor(x => x.PriceMin)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PriceMin.HasValue)
            .WithMessage("Price minimum must be greater than or equal to zero.");

        RuleFor(x => x.PriceMax)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PriceMax.HasValue)
            .WithMessage("Price maximum must be greater than or equal to zero.");

        RuleFor(x => x.PriceMax)
            .GreaterThan(x => x.PriceMin)
            .When(x => x.PriceMin.HasValue && x.PriceMax.HasValue)
            .WithMessage("Price maximum must be greater than price minimum.");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .WithMessage("Invalid sort field. Valid fields are: name, price, createdAt, updatedAt, isActive, isPopular.");

        RuleFor(x => x.BillingCycle)
            .Must(billingCycle => billingCycle == null || BeValidBillingCycle(billingCycle!))
            .When(x => !string.IsNullOrEmpty(x.BillingCycle))
            .WithMessage("Invalid billing cycle. Valid values are: monthly, yearly, lifetime.");
    }

    private static bool BeValidSortField(string sortBy)
    {
        var validFields = new[] { "name", "price", "createdAt", "updatedAt", "isActive", "isPopular" };
        return validFields.Contains(sortBy.ToLowerInvariant());
    }

    private static bool BeValidBillingCycle(string billingCycle)
    {
        var validCycles = new[] { "monthly", "yearly", "lifetime" };
        return validCycles.Contains(billingCycle.ToLowerInvariant());
    }
}

public class GetPlansQueryHandler(ISuperAdminService superAdminService) : IRequestHandler<GetPlansQuery, Result<PaginatedResult<PlanResponseDto>>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<PaginatedResult<PlanResponseDto>>> Handle(GetPlansQuery request, CancellationToken cancellationToken)
    {
        var planListRequest = new PlanListRequest
        {
            Query = request.Query,
            IsActive = request.IsActive,
            BillingCycle = request.BillingCycle,
            PriceMin = request.PriceMin,
            PriceMax = request.PriceMax,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        return await _superAdminService.GetPlansAsync(planListRequest);
    }
}
