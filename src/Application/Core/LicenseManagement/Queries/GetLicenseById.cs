using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;
using HotelManagement.Application.Common.Interfaces.Services;

namespace HotelManagement.Application.Core.LicenseManagement.Queries;

public record GetLicenseByIdQuery : IRequest<Result<LicenseResponseDto>>
{
    public Guid LicenseId { get; init; }
}

public class GetLicenseByIdQueryValidator : AbstractValidator<GetLicenseByIdQuery>
{
    public GetLicenseByIdQueryValidator()
    {
        RuleFor(x => x.LicenseId)
            .NotEmpty()
            .WithMessage("License ID is required.");
    }
}

public class GetLicenseByIdQueryHandler(ILicenseService service) : IRequestHandler<GetLicenseByIdQuery, Result<LicenseResponseDto>>
{
    private readonly ILicenseService _service = service;

    public async Task<Result<LicenseResponseDto>> Handle(GetLicenseByIdQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetLicenseByIdAsync(request.LicenseId);
    }
}

public record GetLicenseByKeyQuery : IRequest<Result<LicenseResponseDto>>
{
    public string LicenseKey { get; init; } = string.Empty;
}

public class GetLicenseByKeyQueryValidator : AbstractValidator<GetLicenseByKeyQuery>
{
    public GetLicenseByKeyQueryValidator()
    {
        RuleFor(x => x.LicenseKey)
            .NotEmpty()
            .WithMessage("License key is required.");
    }
}

public class GetLicenseByKeyQueryHandler(ILicenseService service) : IRequestHandler<GetLicenseByKeyQuery, Result<LicenseResponseDto>>
{
    private readonly ILicenseService _service = service;

    public async Task<Result<LicenseResponseDto>> Handle(GetLicenseByKeyQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetLicenseByKeyAsync(request.LicenseKey);
    }
}
