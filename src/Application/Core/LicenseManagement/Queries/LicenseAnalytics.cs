using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;
using HotelManagement.Application.Common.Interfaces.Services;

namespace HotelManagement.Application.Core.LicenseManagement.Queries;

public record GetLicenseAnalyticsQuery : IRequest<Result<LicenseAnalyticsDto>>
{
}

public class GetLicenseAnalyticsQueryValidator : AbstractValidator<GetLicenseAnalyticsQuery>
{
    public GetLicenseAnalyticsQueryValidator()
    {
        // No validation needed for analytics queries
    }
}

public class GetLicenseAnalyticsQueryHandler(ILicenseService service) : IRequestHandler<GetLicenseAnalyticsQuery, Result<LicenseAnalyticsDto>>
{
    private readonly ILicenseService _service = service;

    public async Task<Result<LicenseAnalyticsDto>> Handle(GetLicenseAnalyticsQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetLicenseAnalyticsAsync();
    }
}

public record GetLicenseUsageQuery : IRequest<Result<LicenseUsageDto[]>>
{
}

public class GetLicenseUsageQueryValidator : AbstractValidator<GetLicenseUsageQuery>
{
    public GetLicenseUsageQueryValidator()
    {
        // No validation needed for analytics queries
    }
}

public class GetLicenseUsageQueryHandler(ILicenseService service) : IRequestHandler<GetLicenseUsageQuery, Result<LicenseUsageDto[]>>
{
    private readonly ILicenseService _service = service;

    public async Task<Result<LicenseUsageDto[]>> Handle(GetLicenseUsageQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetLicenseUsageAsync();
    }
}

public record GetExpiringLicensesQuery : IRequest<Result<LicenseResponseDto[]>>
{
    public int Days { get; init; } = 30;
}

public class GetExpiringLicensesQueryValidator : AbstractValidator<GetExpiringLicensesQuery>
{
    public GetExpiringLicensesQueryValidator()
    {
        RuleFor(x => x.Days)
            .InclusiveBetween(1, 365)
            .WithMessage("Days must be between 1 and 365.");
    }
}

public class GetExpiringLicensesQueryHandler(ILicenseService service) : IRequestHandler<GetExpiringLicensesQuery, Result<LicenseResponseDto[]>>
{
    private readonly ILicenseService _service = service;

    public async Task<Result<LicenseResponseDto[]>> Handle(GetExpiringLicensesQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetExpiringLicensesAsync(request.Days);
    }
}
