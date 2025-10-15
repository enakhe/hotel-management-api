using HotelManagement.Application.Common.Interfaces;

namespace HotelManagement.Application.Tenant.Queries.GetTenantById;

public record GetTenantByIdQuery : IRequest<Task<Result<TenantDetail>>>
{
}

public class GetTenantByIdQueryValidator : AbstractValidator<GetTenantByIdQuery>
{
    public GetTenantByIdQueryValidator()
    {
    }
}

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Task<Result<TenantDetail>>>
{
    private readonly IApplicationDbContext _context;

    public GetTenantByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Task<Result<TenantDetail>>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
