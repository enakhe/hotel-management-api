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

        // Map from CreateTenantCommand to CreateTenantRequest
        CreateMap<CreateTenantCommand, CreateTenantRequest>()
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.Region, opt => opt.Ignore())
            .ForMember(dest => dest.Industry, opt => opt.Ignore())
            .ForMember(dest => dest.LicenseStatus, opt => opt.MapFrom(src => src.LicenseExpiryDate.HasValue ?
                (src.LicenseExpiryDate > DateTime.UtcNow ? LicenseStatus.Active : LicenseStatus.Expired) :
                LicenseStatus.Trial))
            .ForMember(dest => dest.EnabledModules, opt => opt.MapFrom(src => new string[0]))
            .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone ?? "UTC"))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode ?? "USD"));
    }
}
