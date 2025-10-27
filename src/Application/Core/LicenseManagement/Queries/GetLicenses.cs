using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;
using HotelManagement.Domain.Enums;
using HotelManagement.Application.Common.Interfaces.Services;

namespace HotelManagement.Application.Core.LicenseManagement.Queries;

public record GetLicensesQuery : IRequest<Result<PaginatedResult<LicenseResponseDto>>>
{
    public string? Query { get; init; }
    public LicenseStatusType? Status { get; init; }
    public LicenseType? Type { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? PlanId { get; init; }
    public bool? Expired { get; init; }
    public int? ExpiresInDays { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public string SortBy { get; init; } = "created";
    public bool SortDescending { get; init; } = true;
}

public class GetLicensesQueryValidator : AbstractValidator<GetLicensesQuery>
{
    public GetLicensesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, 100)
            .WithMessage("Size must be between 1 and 100.");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .WithMessage("Invalid sort field. Valid fields are: licensekey, tenantname, planname, status, type, issueddate, expirationdate, validationcount, created, lastmodified.");

        RuleFor(x => x.ExpiresInDays)
            .GreaterThan(0)
            .When(x => x.ExpiresInDays.HasValue)
            .WithMessage("Expires in days must be greater than 0.");
    }

    private static bool BeValidSortField(string? sortField)
    {
        if (string.IsNullOrEmpty(sortField))
            return true;

        var validFields = new[]
        {
            "licensekey", "tenantname", "planname", "status", "type",
            "issueddate", "expirationdate", "validationcount", "created", "lastmodified"
        };

        return validFields.Contains(sortField.ToLowerInvariant());
    }
}

public class GetLicensesQueryHandler(ILicenseService service) : IRequestHandler<GetLicensesQuery, Result<PaginatedResult<LicenseResponseDto>>>
{
    private readonly ILicenseService _service = service;

    public async Task<Result<PaginatedResult<LicenseResponseDto>>> Handle(GetLicensesQuery request, CancellationToken cancellationToken)
    {
        var listRequest = new LicenseListRequest
        {
            Query = request.Query,
            Status = request.Status,
            Type = request.Type,
            TenantId = request.TenantId,
            PlanId = request.PlanId,
            Expired = request.Expired,
            ExpiresInDays = request.ExpiresInDays,
            Page = request.Page,
            Size = request.Size,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        return await _service.GetLicensesAsync(listRequest);
    }
}
