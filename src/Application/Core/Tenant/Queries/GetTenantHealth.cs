using HotelManagement.Application.Common.DTOs;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Core.Tenant.Queries;

public record GetTenantHealthCommand : IRequest<Result<TenantHealth>>
{
    public Guid TenantId { get; init; }
}

public class GetTenantHealthCommandValidator : AbstractValidator<GetTenantHealthCommand>
{
    public GetTenantHealthCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId is required.");
    }
}

public class GetTenantHealthCommandHandler(ITenantService service) : IRequestHandler<GetTenantHealthCommand, Result<TenantHealth>>
{
    private readonly ITenantService _service = service;

    public async Task<Result<TenantHealth>> Handle(GetTenantHealthCommand request, CancellationToken cancellationToken)
    {
        return await _service.GetTenantHealthAsync(request.TenantId);
    }
}
