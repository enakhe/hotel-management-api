using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Core.ModuleManagement.Commands;

public record CreateModuleCommand : IRequest<Result<ModuleResponseDto>>
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Category { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsCore { get; init; } = false;
    public string[] Dependencies { get; init; } = [];
    public required CreateModuleFeatureCommand[] Features { get; init; }
    public required CreateModulePricingCommand Pricing { get; init; }
}

public record CreateModuleFeatureCommand
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool IsEnabled { get; init; } = true;
    public Dictionary<string, object>? Configuration { get; init; }
}

public record CreateModulePricingCommand
{
    public PricingType Type { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
    public BillingCycle? BillingCycle { get; init; }
    public int? MinQuantity { get; init; }
    public int? MaxQuantity { get; init; }
}

public class CreateModuleCommandValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Module name is required.")
            .MaximumLength(100).WithMessage("Module name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(50).WithMessage("Category cannot exceed 50 characters.");

        RuleFor(x => x.Features)
            .NotEmpty().WithMessage("At least one feature is required.");

        RuleForEach(x => x.Features).SetValidator(new CreateModuleFeatureCommandValidator());
        RuleFor(x => x.Pricing).SetValidator(new CreateModulePricingCommandValidator());
    }
}

public class CreateModuleFeatureCommandValidator : AbstractValidator<CreateModuleFeatureCommand>
{
    public CreateModuleFeatureCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Feature name is required.")
            .MaximumLength(100).WithMessage("Feature name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Feature description cannot exceed 500 characters.");
    }
}

public class CreateModulePricingCommandValidator : AbstractValidator<CreateModulePricingCommand>
{
    public CreateModulePricingCommandValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid pricing type.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).When(x => x.Price.HasValue)
            .WithMessage("Price must be a non-negative value.");

        RuleFor(x => x.Currency)
            .Length(3).When(x => !string.IsNullOrEmpty(x.Currency))
            .WithMessage("Currency code must be 3 characters (e.g., USD).");

        RuleFor(x => x.BillingCycle)
            .IsInEnum().When(x => x.BillingCycle.HasValue)
            .WithMessage("Invalid billing cycle.");

        RuleFor(x => x.MinQuantity)
            .GreaterThanOrEqualTo(0).When(x => x.MinQuantity.HasValue)
            .WithMessage("Minimum quantity must be a non-negative value.");

        RuleFor(x => x.MaxQuantity)
            .GreaterThanOrEqualTo(0).When(x => x.MaxQuantity.HasValue)
            .WithMessage("Maximum quantity must be a non-negative value.");

        RuleFor(x => x.MaxQuantity)
            .GreaterThanOrEqualTo(x => x.MinQuantity)
            .When(x => x.MinQuantity.HasValue && x.MaxQuantity.HasValue)
            .WithMessage("Maximum quantity must be greater than or equal to minimum quantity.");
    }
}

public class CreateModuleCommandHandler(IModuleService service, IMapper mapper) : IRequestHandler<CreateModuleCommand, Result<ModuleResponseDto>>
{
    private readonly IModuleService _service = service;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ModuleResponseDto>> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        var moduleRequest = _mapper.Map<CreateModuleRequest>(request);
        return await _service.CreateModuleAsync(moduleRequest);
    }
}
