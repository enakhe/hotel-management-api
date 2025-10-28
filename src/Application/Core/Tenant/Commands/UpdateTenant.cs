using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Tenant.Commands;

public record UpdateTenantCommand : IRequest<Result<bool>>
{
    public Guid TenantId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Address { get; init; }
    public string? ContactNumber { get; init; }
    public string? Email { get; init; }
    public string? TimeZone { get; init; }
    public string? CurrencyCode { get; init; }
    public string? LanguageCode { get; init; }
    public string? Country { get; init; }
    public string? Region { get; init; }
    public string? Industry { get; init; }
    public Guid PlanId { get; init; }
    public Guid LicenseId { get; init; }
}

public class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(v => v.TenantId).NotEmpty().WithMessage("TenantId is required");
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tenant name is required.")
            .MaximumLength(100).WithMessage("Tenant name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format.");

        RuleFor(x => x.ContactNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.ContactNumber))
            .WithMessage("Invalid contact number format.");

        RuleFor(x => x.TimeZone)
            .NotEmpty()
            .WithMessage("Time zone is required.");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty()
            .WithMessage("Currency code is required.")
            .Length(3)
            .WithMessage("Currency code must be a 3-letter ISO code.");

        RuleFor(x => x.LanguageCode)
            .NotEmpty()
            .WithMessage("Language code is required.")
            .Length(2).WithMessage("Language code must be a 2-letter ISO code.");
    }
}

public class UpdateTenantCommandHandler(ITenantService service, IMapper mapper) : IRequestHandler<UpdateTenantCommand, Result<bool>>
{
    private readonly ITenantService _service = service;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<bool>> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = _mapper.Map<UpdateTenantRequest>(request);
        return await _service.UpdateTenantAsync(request.TenantId, updateRequest);
    }
}
