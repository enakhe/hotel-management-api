using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.TenantAdminManagement.Commands;

/// <summary>
/// Command to update a Tenant Administrator's profile
/// </summary>
public record UpdateTenantAdminCommand : IRequest<Result<TenantAdminDto>>
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public string? PhoneNumber { get; init; }
}

public class UpdateTenantAdminCommandValidator : AbstractValidator<UpdateTenantAdminCommand>
{
    public UpdateTenantAdminCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("User ID is required.")
            .NotEqual(Guid.Empty).WithMessage("User ID cannot be empty.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.MiddleName)
            .NotEmpty().WithMessage("Middle name is required.")
            .MaximumLength(50).WithMessage("Middle name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Invalid phone number format.");
    }
}

public class UpdateTenantAdminCommandHandler : IRequestHandler<UpdateTenantAdminCommand, Result<TenantAdminDto>>
{
    private readonly ITenantAdminService _tenantAdminService;
    private readonly IMapper _mapper;

    public UpdateTenantAdminCommandHandler(ITenantAdminService tenantAdminService, IMapper mapper)
    {
        _tenantAdminService = tenantAdminService;
        _mapper = mapper;
    }

    public async Task<Result<TenantAdminDto>> Handle(UpdateTenantAdminCommand request, CancellationToken cancellationToken)
    {
        var dto = _mapper.Map<UpdateTenantAdminDto>(request);
        return await _tenantAdminService.UpdateTenantAdminAsync(dto);
    }
}

