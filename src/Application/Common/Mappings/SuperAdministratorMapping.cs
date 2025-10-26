using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Core.Tenant.Commands;
using HotelManagement.Domain.Entities.Configuration;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.Mappings;

public class SuperAdministratorMappingProfile : Profile
{
    public SuperAdministratorMappingProfile()
    {
        CreateMap<CreateTenantRequest, Domain.Entities.Configuration.Tenant>();
        CreateMap<Domain.Entities.Configuration.Tenant, CreateTenantRequest>();

        CreateMap<UpdateTenantRequest, Domain.Entities.Configuration.Tenant>();
        CreateMap<Domain.Entities.Configuration.Tenant, UpdateTenantRequest>();

        // Map from CreateTenantCommand to CreateTenantRequest
        CreateMap<CreateTenantCommand, CreateTenantRequest>()
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.Region, opt => opt.Ignore())
            .ForMember(dest => dest.Industry, opt => opt.Ignore())
            .ForMember(dest => dest.Modules, opt => opt.MapFrom(src => src.Modules ?? Array.Empty<string>()))
            .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone ?? "UTC"))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode ?? "USD"));

        CreateMap<Domain.Entities.Configuration.Tenant, TenantSummary>()
            .ForMember(dest => dest.Modules, opt => opt.MapFrom(src => src.Features.Where(f => f.IsEnabled).Select(f => f.FeatureName).ToArray()));

        CreateMap<Domain.Entities.Configuration.Tenant, TenantDetail>()
            .ForMember(dest => dest.EnabledModules, opt => opt.MapFrom(src => src.Features.Where(f => f.IsEnabled).Select(f => f.FeatureName).ToArray()));
    }
}
