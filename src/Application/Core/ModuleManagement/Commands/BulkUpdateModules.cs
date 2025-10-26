using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace HotelManagement.Application.Core.ModuleManagement.Commands;

public record BulkUpdateModulesCommand : IRequest<Result<bool>>
{
    public required BulkModuleUpdateRequest[] Updates { get; init; }
}

public record BulkModuleUpdateRequest
{
    public Guid Id { get; init; }
    public required UpdateModuleRequest Data { get; init; }
}

public class BulkUpdateModulesCommandValidator : AbstractValidator<BulkUpdateModulesCommand>
{
    public BulkUpdateModulesCommandValidator()
    {
        RuleFor(x => x.Updates)
            .NotEmpty()
            .WithMessage("At least one module update is required.");

        RuleFor(x => x.Updates)
            .Must(updates => updates.Length <= 100)
            .WithMessage("Cannot update more than 100 modules at once.");

        RuleForEach(x => x.Updates)
            .SetValidator(new BulkModuleUpdateRequestValidator());
    }
}

public class BulkModuleUpdateRequestValidator : AbstractValidator<BulkModuleUpdateRequest>
{
    public BulkModuleUpdateRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Module ID is required.");
    }
}

public class BulkUpdateModulesCommandHandler(ISuperAdminService superAdminService) : IRequestHandler<BulkUpdateModulesCommand, Result<bool>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<bool>> Handle(BulkUpdateModulesCommand request, CancellationToken cancellationToken)
    {
        return await _superAdminService.BulkUpdateModulesAsync(request.Updates);
    }
}
