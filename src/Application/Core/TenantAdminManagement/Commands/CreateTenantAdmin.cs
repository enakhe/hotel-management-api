using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.TenantAdminManagement.Commands;

/// <summary>
/// Command to create a new Tenant Administrator
/// </summary>
public record CreateTenantAdminCommand : IRequest<Result<TenantAdminDto>>
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public required string Password { get; init; }
    public required Guid TenantId { get; init; }
}

public class CreateTenantAdminCommandValidator : AbstractValidator<CreateTenantAdminCommand>
{
    public CreateTenantAdminCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.MiddleName)
            .NotEmpty().WithMessage("Middle name is required.")
            .MaximumLength(50).WithMessage("Middle name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Invalid phone number format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Tenant ID cannot be empty.");
    }
}

public class CreateTenantAdminCommandHandler : IRequestHandler<CreateTenantAdminCommand, Result<TenantAdminDto>>
{
    private readonly ITenantAdminService _tenantAdminService;
    private readonly IMapper _mapper;

    public CreateTenantAdminCommandHandler(ITenantAdminService tenantAdminService, IMapper mapper)
    {
        _tenantAdminService = tenantAdminService;
        _mapper = mapper;
    }

    public async Task<Result<TenantAdminDto>> Handle(CreateTenantAdminCommand request, CancellationToken cancellationToken)
    {
        var dto = _mapper.Map<CreateTenantAdminDto>(request);
        return await _tenantAdminService.CreateTenantAdminAsync(dto);
    }
}

