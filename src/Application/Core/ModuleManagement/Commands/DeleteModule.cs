using HotelManagement.Application.Common.Interfaces.Services;
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

public class DeleteModuleCommandHandler(IModuleService service) : IRequestHandler<DeleteModuleCommand, Result<bool>>
{
    private readonly IModuleService _service = service;

    public async Task<Result<bool>> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
    {
        return await _service.DeleteModuleAsync(request.ModuleId);
    }
}
