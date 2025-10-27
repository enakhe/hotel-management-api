using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.Services;
using HotelManagement.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace HotelManagement.Application.Core.ModuleManagement.Queries;

public record GetModuleByIdQuery : IRequest<Result<ModuleResponseDto>>
{
    public Guid ModuleId { get; init; }
}

public class GetModuleByIdQueryValidator : AbstractValidator<GetModuleByIdQuery>
{
    public GetModuleByIdQueryValidator()
    {
        RuleFor(x => x.ModuleId)
            .NotEmpty()
            .WithMessage("Module ID is required.");
    }
}

public class GetModuleByIdQueryHandler(IModuleService service) : IRequestHandler<GetModuleByIdQuery, Result<ModuleResponseDto>>
{
    private readonly IModuleService _service = service;

    public async Task<Result<ModuleResponseDto>> Handle(GetModuleByIdQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetModuleByIdAsync(request.ModuleId);
    }
}
