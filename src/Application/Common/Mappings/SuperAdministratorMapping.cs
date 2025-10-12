using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Domain.Entities.Configuration;

namespace HotelManagement.Application.Common.Mappings;

public class SuperAdministratorMappingProfile : Profile
{
    public SuperAdministratorMappingProfile()
    {
        CreateMap<CreateTenantRequest, Tenant>();
        CreateMap<Tenant, CreateTenantRequest>();
    }
}
