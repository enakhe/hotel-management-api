using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.Services;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using AutoMapper;

namespace HotelManagement.Application.Core.ModuleManagement.Commands;

public record UpdateModuleCommand : IRequest<Result<ModuleResponseDto>>
{
    public Guid ModuleId { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsCore { get; init; }
    public string[]? Dependencies { get; init; }
    public UpdateModuleFeatureCommand[]? Features { get; init; }
    public UpdateModulePricingCommand? Pricing { get; init; }
}

public record UpdateModuleFeatureCommand
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsEnabled { get; init; }
    public Dictionary<string, object>? Configuration { get; init; }
}

public record UpdateModulePricingCommand
{
    public PricingType? Type { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
    public BillingCycle? BillingCycle { get; init; }
    public int? MinQuantity { get; init; }
    public int? MaxQuantity { get; init; }
}

public class UpdateModuleCommandValidator : AbstractValidator<UpdateModuleCommand>
{
    public UpdateModuleCommandValidator()
    {
        RuleFor(x => x.ModuleId)
            .NotEmpty().WithMessage("Module ID is required.");

        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Module name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Category)
            .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Category))
            .WithMessage("Category cannot exceed 50 characters.");

        RuleForEach(x => x.Features)
            .SetValidator(new UpdateModuleFeatureCommandValidator())
            .When(x => x.Features != null && x.Features.Length > 0);

        RuleFor(x => x.Pricing)
            .SetValidator(new UpdateModulePricingCommandValidator())
            .When(x => x.Pricing != null);
    }
}

public class UpdateModuleFeatureCommandValidator : AbstractValidator<UpdateModuleFeatureCommand>
{
    public UpdateModuleFeatureCommandValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Feature name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Feature description cannot exceed 500 characters.");
    }
}

public class UpdateModulePricingCommandValidator : AbstractValidator<UpdateModulePricingCommand?>
{
    public UpdateModulePricingCommandValidator()
    {
        RuleFor(x => x!.Type)
            .IsInEnum().When(x => x != null && x.Type.HasValue)
            .WithMessage("Invalid pricing type.");

        RuleFor(x => x!.Price)
            .GreaterThanOrEqualTo(0).When(x => x != null && x.Price.HasValue)
            .WithMessage("Price must be a non-negative value.");

        RuleFor(x => x!.Currency)
            .Length(3).When(x => x != null && !string.IsNullOrEmpty(x.Currency))
            .WithMessage("Currency code must be 3 characters (e.g., USD).");

        RuleFor(x => x!.BillingCycle)
            .IsInEnum().When(x => x != null && x.BillingCycle.HasValue)
            .WithMessage("Invalid billing cycle.");

        RuleFor(x => x!.MinQuantity)
            .GreaterThanOrEqualTo(0).When(x => x != null && x.MinQuantity.HasValue)
            .WithMessage("Minimum quantity must be a non-negative value.");

        RuleFor(x => x!.MaxQuantity)
            .GreaterThanOrEqualTo(0).When(x => x != null && x.MaxQuantity.HasValue)
            .WithMessage("Maximum quantity must be a non-negative value.");

        RuleFor(x => x!.MaxQuantity)
            .GreaterThanOrEqualTo(x => x!.MinQuantity)
            .When(x => x != null && x.MinQuantity.HasValue && x.MaxQuantity.HasValue)
            .WithMessage("Maximum quantity must be greater than or equal to minimum quantity.");
    }
}

public class UpdateModuleCommandHandler(IModuleService service, IMapper mapper) : IRequestHandler<UpdateModuleCommand, Result<ModuleResponseDto>>
{
    private readonly IModuleService _service = service;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ModuleResponseDto>> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = _mapper.Map<UpdateModuleRequest>(request);
        return await _service.UpdateModuleAsync(request.ModuleId, updateRequest);
    }
}
