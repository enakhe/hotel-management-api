using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.Services;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace HotelManagement.Application.Core.ModuleManagement.Queries;

public record GetModulesQuery : IRequest<Result<PaginatedResult<ModuleResponseDto>>>
{
    public string? Query { get; init; }
    public string? Category { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsCore { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public string SortBy { get; init; } = "createdAt";
    public bool SortDescending { get; init; } = true;
}

public class GetModulesQueryValidator : AbstractValidator<GetModulesQuery>
{
    public GetModulesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than zero.");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100)
            .WithMessage("Size must be between 1 and 100.");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .WithMessage("Invalid sort field. Valid fields are: name, category, createdat, updatedat, isactive, iscore.");
    }

    private static bool BeValidSortField(string sortBy)
    {
        var validFields = new[] { "name", "category", "createdat", "updatedat", "isactive", "iscore" };
        return validFields.Contains(sortBy.ToLowerInvariant());
    }
}

public class GetModulesQueryHandler(IModuleService service) : IRequestHandler<GetModulesQuery, Result<PaginatedResult<ModuleResponseDto>>>
{
    private readonly IModuleService _service = service;

    public async Task<Result<PaginatedResult<ModuleResponseDto>>> Handle(GetModulesQuery request, CancellationToken cancellationToken)
    {
        var moduleListRequest = new ModuleListRequest
        {
            Query = request.Query,
            Category = request.Category,
            IsActive = request.IsActive,
            IsCore = request.IsCore,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        return await _service.GetModulesAsync(moduleListRequest);
    }
}
