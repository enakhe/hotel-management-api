using AutoMapper;
using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Domain.Entities.Configuration;
using HotelManagement.Domain.Entities.SuperAdmin;

namespace HotelManagement.Application.Common.Mappings;

/// <summary>
/// Limits mapping profile
/// </summary>
public class LimitsMappingProfile : Profile
{
    public LimitsMappingProfile()
    {
        CreateMap<Limits, LimitsResponseDto>()
            .ForMember(dest => dest.CustomLimits, opt => opt.MapFrom(src =>
                DeserializeCustomLimits(src.CustomLimits)));

        CreateMap<CreateLimitsRequest, Limits>()
            .ForMember(dest => dest.CustomLimits, opt => opt.MapFrom(src =>
                SerializeCustomLimits(src.CustomLimits)))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => "System"))
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Plans, opt => opt.Ignore())
            .ForMember(dest => dest.TenantOverrides, opt => opt.Ignore());

        CreateMap<UpdateLimitsRequest, Limits>()
            .ForMember(dest => dest.CustomLimits, opt => opt.MapFrom(src =>
                SerializeCustomLimits(src.CustomLimits)))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => "System"))
            .ForMember(dest => dest.Plans, opt => opt.Ignore())
            .ForMember(dest => dest.TenantOverrides, opt => opt.Ignore());
    }

    private static Dictionary<string, int>? DeserializeCustomLimits(string? customLimits)
    {
        return string.IsNullOrEmpty(customLimits) ? null :
            System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(customLimits);
    }

    private static string? SerializeCustomLimits(Dictionary<string, int>? customLimits)
    {
        return customLimits == null ? null :
            System.Text.Json.JsonSerializer.Serialize(customLimits);
    }
}

/// <summary>
/// Plan mapping profile
/// </summary>
public class PlanMappingProfile : Profile
{
    public PlanMappingProfile()
    {
        CreateMap<Plan, PlanResponseDto>()
            .ForMember(dest => dest.Modules, opt => opt.MapFrom(src => src.PlanModules.Select(pm => pm.Module)))
            .ForMember(dest => dest.Limits, opt => opt.MapFrom(src => src.Limits));

        CreateMap<CreatePlanRequest, Plan>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => "System"))
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Limits, opt => opt.Ignore())
            .ForMember(dest => dest.Tenants, opt => opt.Ignore())
            .ForMember(dest => dest.Licenses, opt => opt.Ignore())
            .ForMember(dest => dest.PlanModules, opt => opt.Ignore());

        CreateMap<UpdatePlanRequest, Plan>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => "System"))
            .ForMember(dest => dest.Limits, opt => opt.Ignore())
            .ForMember(dest => dest.Tenants, opt => opt.Ignore())
            .ForMember(dest => dest.Licenses, opt => opt.Ignore())
            .ForMember(dest => dest.PlanModules, opt => opt.Ignore());
    }
}

/// <summary>
/// Tenant mapping profile
/// </summary>
public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        CreateMap<Domain.Entities.Configuration.Tenant, TenantResponseDto>()
            .ForMember(dest => dest.Plan, opt => opt.MapFrom(src => src.Plan));

        CreateMap<CreateTenantRequest, Domain.Entities.Configuration.Tenant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.MapFrom(src => DateTimeOffset.UtcNow))
            .ForMember(dest => dest.LastModified, opt => opt.MapFrom(src => DateTimeOffset.UtcNow))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => "System"))
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Plan, opt => opt.Ignore())
            .ForMember(dest => dest.License, opt => opt.Ignore())
            .ForMember(dest => dest.Branches, opt => opt.Ignore());

        CreateMap<UpdateTenantRequest, Domain.Entities.Configuration.Tenant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Created, opt => opt.Ignore())
            .ForMember(dest => dest.LastModified, opt => opt.MapFrom(src => DateTimeOffset.UtcNow))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => "System"))
            .ForMember(dest => dest.Plan, opt => opt.Ignore())
            .ForMember(dest => dest.License, opt => opt.Ignore())
            .ForMember(dest => dest.Branches, opt => opt.Ignore());
    }
}

/// <summary>
/// License mapping profile
/// </summary>
public class LicenseMappingProfile : Profile
{
    public LicenseMappingProfile()
    {
        CreateMap<License, LicenseResponseDto>()
            .ForMember(dest => dest.DomainRestrictions, opt => opt.MapFrom(src =>
                DeserializeStringArray(src.DomainRestrictions)))
            .ForMember(dest => dest.IpRestrictions, opt => opt.MapFrom(src =>
                DeserializeStringArray(src.IpRestrictions)))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                DeserializeMetadata(src.Metadata)))
            .ForMember(dest => dest.Limits, opt => opt.MapFrom(src => src.Plan.Limits));

        CreateMap<CreateLicenseRequest, License>()
            .ForMember(dest => dest.DomainRestrictions, opt => opt.MapFrom(src =>
                SerializeStringArray(src.DomainRestrictions)))
            .ForMember(dest => dest.IpRestrictions, opt => opt.MapFrom(src =>
                SerializeStringArray(src.IpRestrictions)))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                SerializeMetadata(src.Metadata)))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IssuedDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => "System"))
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Plan, opt => opt.Ignore())
            .ForMember(dest => dest.Validations, opt => opt.Ignore());

        CreateMap<UpdateLicenseRequest, License>()
            .ForMember(dest => dest.DomainRestrictions, opt => opt.MapFrom(src =>
                SerializeStringArray(src.DomainRestrictions)))
            .ForMember(dest => dest.IpRestrictions, opt => opt.MapFrom(src =>
                SerializeStringArray(src.IpRestrictions)))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                SerializeMetadata(src.Metadata)))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IssuedDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.MapFrom(src => "System"))
            .ForMember(dest => dest.Plan, opt => opt.Ignore())
            .ForMember(dest => dest.Validations, opt => opt.Ignore());
    }

    private static string[]? DeserializeStringArray(string? json)
    {
        return string.IsNullOrEmpty(json) ? null :
            System.Text.Json.JsonSerializer.Deserialize<string[]>(json);
    }

    private static string? SerializeStringArray(string[]? array)
    {
        return array == null ? null :
            System.Text.Json.JsonSerializer.Serialize(array);
    }

    private static Dictionary<string, object>? DeserializeMetadata(string? json)
    {
        return string.IsNullOrEmpty(json) ? null :
            System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
    }

    private static string? SerializeMetadata(Dictionary<string, object>? metadata)
    {
        return metadata == null ? null :
            System.Text.Json.JsonSerializer.Serialize(metadata);
    }
}
