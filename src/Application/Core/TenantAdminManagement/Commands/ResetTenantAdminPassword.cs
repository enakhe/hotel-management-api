using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.TenantAdminManagement.Commands;

/// <summary>
/// Command to reset a Tenant Administrator's password
/// </summary>
public record ResetTenantAdminPasswordCommand : IRequest<Result<bool>>
{
    public required Guid UserId { get; init; }
    public required string NewPassword { get; init; }
}

public class ResetTenantAdminPasswordCommandValidator : AbstractValidator<ResetTenantAdminPasswordCommand>
{
    public ResetTenantAdminPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}

public class ResetTenantAdminPasswordCommandHandler : IRequestHandler<ResetTenantAdminPasswordCommand, Result<bool>>
{
    private readonly ITenantAdminService _tenantAdminService;
    private readonly IMapper _mapper;

    public ResetTenantAdminPasswordCommandHandler(ITenantAdminService tenantAdminService, IMapper mapper)
    {
        _tenantAdminService = tenantAdminService;
        _mapper = mapper;
    }

    public async Task<Result<bool>> Handle(ResetTenantAdminPasswordCommand request, CancellationToken cancellationToken)
    {
        var dto = _mapper.Map<ResetTenantAdminPasswordDto>(request);
        return await _tenantAdminService.ResetPasswordAsync(dto);
    }
}

