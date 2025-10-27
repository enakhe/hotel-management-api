using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;
using HotelManagement.Application.Common.Interfaces.Services;

namespace HotelManagement.Application.Core.LicenseManagement.Commands;

public record RenewLicenseCommand : IRequest<Result<LicenseResponseDto>>
{
    public Guid LicenseId { get; init; }
    public DateTime NewExpirationDate { get; init; }
    public string Reason { get; init; } = string.Empty;
    public CreateLicenseFeatureRequest[]? ExtendFeatures { get; init; }
    public CreateLicenseLimitsRequest? ExtendLimits { get; init; }
}

public class RenewLicenseCommandValidator : AbstractValidator<RenewLicenseCommand>
{
    public RenewLicenseCommandValidator()
    {
        RuleFor(x => x.LicenseId)
            .NotEmpty()
            .WithMessage("License ID is required.");

        RuleFor(x => x.NewExpirationDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("New expiration date must be in the future.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Reason is required and cannot exceed 500 characters.");
    }
}

public class RenewLicenseCommandHandler(ILicenseService service) : IRequestHandler<RenewLicenseCommand, Result<LicenseResponseDto>>
{
    private readonly ILicenseService _service = service;

    public async Task<Result<LicenseResponseDto>> Handle(RenewLicenseCommand request, CancellationToken cancellationToken)
    {
        var renewalRequest = new LicenseRenewalRequest
        {
            LicenseId = request.LicenseId,
            NewExpirationDate = request.NewExpirationDate,
            Reason = request.Reason,
            ExtendFeatures = [.. request.ExtendFeatures!.Select(x => new CreateLicenseFeatureRequest
            {
                Name = x.Name,
                Description = x.Description,
                Enabled = x.Enabled,
                Limit = x.Limit,
                Unit = x.Unit,
                Configuration = x.Configuration
            })],
            ExtendLimits = new CreateLicenseLimitsRequest
            {
                MaxUsers = request.ExtendLimits!.MaxUsers,
                MaxBranches = request.ExtendLimits!.MaxBranches,
                MaxRooms = request.ExtendLimits!.MaxRooms,
                MaxReservations = request.ExtendLimits!.MaxReservations,
                MaxStorageGB = request.ExtendLimits!.MaxStorageGB,
                ApiRateLimit = request.ExtendLimits!.ApiRateLimit,
                ConcurrentSessions = request.ExtendLimits!.ConcurrentSessions,
                CustomLimits = request.ExtendLimits!.CustomLimits
            }
        };

        return await _service.RenewLicenseAsync(request.LicenseId, renewalRequest);
    }
}

public record TransferLicenseCommand : IRequest<Result<LicenseResponseDto>>
{
    public Guid LicenseId { get; init; }
    public Guid NewTenantId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime TransferDate { get; init; }
}

public class TransferLicenseCommandValidator : AbstractValidator<TransferLicenseCommand>
{
    public TransferLicenseCommandValidator()
    {
        RuleFor(x => x.LicenseId)
            .NotEmpty()
            .WithMessage("License ID is required.");

        RuleFor(x => x.NewTenantId)
            .NotEmpty()
            .WithMessage("New tenant ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Reason is required and cannot exceed 500 characters.");

        RuleFor(x => x.TransferDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Transfer date cannot be in the past.");
    }
}

public class TransferLicenseCommandHandler(ILicenseService service) : IRequestHandler<TransferLicenseCommand, Result<LicenseResponseDto>>
{
    private readonly ILicenseService _service = service;

    public async Task<Result<LicenseResponseDto>> Handle(TransferLicenseCommand request, CancellationToken cancellationToken)
    {
        var transferRequest = new LicenseTransferRequest
        {
            LicenseId = request.LicenseId,
            NewTenantId = request.NewTenantId,
            Reason = request.Reason,
            TransferDate = request.TransferDate
        };

        return await _service.TransferLicenseAsync(request.LicenseId, transferRequest);
    }
}

public record SuspendLicenseCommand : IRequest<Result<LicenseResponseDto>>
{
    public Guid LicenseId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public class SuspendLicenseCommandValidator : AbstractValidator<SuspendLicenseCommand>
{
    public SuspendLicenseCommandValidator()
    {
        RuleFor(x => x.LicenseId)
            .NotEmpty()
            .WithMessage("License ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Reason is required and cannot exceed 500 characters.");
    }
}

public class SuspendLicenseCommandHandler(ILicenseService service) : IRequestHandler<SuspendLicenseCommand, Result<LicenseResponseDto>>
{
    private readonly ILicenseService _service = service;

    public async Task<Result<LicenseResponseDto>> Handle(SuspendLicenseCommand request, CancellationToken cancellationToken)
    {
        return await _service.SuspendLicenseAsync(request.LicenseId, request.Reason);
    }
}

public record RevokeLicenseCommand : IRequest<Result<LicenseResponseDto>>
{
    public Guid LicenseId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public class RevokeLicenseCommandValidator : AbstractValidator<RevokeLicenseCommand>
{
    public RevokeLicenseCommandValidator()
    {
        RuleFor(x => x.LicenseId)
            .NotEmpty()
            .WithMessage("License ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Reason is required and cannot exceed 500 characters.");
    }
}

public class RevokeLicenseCommandHandler(ILicenseService service) : IRequestHandler<RevokeLicenseCommand, Result<LicenseResponseDto>>
{
    private readonly ILicenseService _service = service;

    public async Task<Result<LicenseResponseDto>> Handle(RevokeLicenseCommand request, CancellationToken cancellationToken)
    {
        return await _service.RevokeLicenseAsync(request.LicenseId, request.Reason);
    }
}

public record ActivateLicenseCommand : IRequest<Result<LicenseResponseDto>>
{
    public Guid LicenseId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public class ActivateLicenseCommandValidator : AbstractValidator<ActivateLicenseCommand>
{
    public ActivateLicenseCommandValidator()
    {
        RuleFor(x => x.LicenseId)
            .NotEmpty()
            .WithMessage("License ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("Reason is required and cannot exceed 500 characters.");
    }
}

public class ActivateLicenseCommandHandler(ILicenseService service) : IRequestHandler<ActivateLicenseCommand, Result<LicenseResponseDto>>
{
    private readonly ILicenseService _service = service;

    public async Task<Result<LicenseResponseDto>> Handle(ActivateLicenseCommand request, CancellationToken cancellationToken)
    {
        return await _service.ActivateLicenseAsync(request.LicenseId, request.Reason);
    }
}
