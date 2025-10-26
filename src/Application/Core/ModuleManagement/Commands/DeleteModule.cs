using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace HotelManagement.Application.Core.ModuleManagement.Commands;

public record DeleteModuleCommand : IRequest<Result<bool>>
{
    public Guid ModuleId { get; init; }
}

public class DeleteModuleCommandValidator : AbstractValidator<DeleteModuleCommand>
{
    public DeleteModuleCommandValidator()
    {
        RuleFor(x => x.ModuleId)
            .NotEmpty()
            .WithMessage("Module ID is required.");
    }
}

public class DeleteModuleCommandHandler(ISuperAdminService superAdminService) : IRequestHandler<DeleteModuleCommand, Result<bool>>
{
    private readonly ISuperAdminService _superAdminService = superAdminService;

    public async Task<Result<bool>> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
    {
        return await _superAdminService.DeleteModuleAsync(request.ModuleId);
    }
}
