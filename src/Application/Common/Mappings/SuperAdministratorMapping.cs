using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Core.Tenant.Commands;
using HotelManagement.Application.Core.PlanManagement.Commands;
using HotelManagement.Domain.Entities.Configuration;
using HotelManagement.Domain.Entities.SuperAdmin;
using HotelManagement.Domain.Enums;
using System.Text.Json;
using HotelManagement.Application.Core.ModuleManagement.Commands;

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
            .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone ?? "WAT"))
            .ForMember(dest => dest.CurrencyCode, opt => opt.MapFrom(src => src.CurrencyCode ?? "NGN"));

        CreateMap<Domain.Entities.Configuration.Tenant, TenantSummary>()
            .ForMember(dest => dest.Modules, opt => opt.MapFrom(src => src.Features.Where(f => f.IsEnabled).Select(f => f.FeatureName).ToArray()));

        CreateMap<Domain.Entities.Configuration.Tenant, TenantDetail>()
            .ForMember(dest => dest.EnabledModules, opt => opt.MapFrom(src => src.Features.Where(f => f.IsEnabled).Select(f => f.FeatureName).ToArray()));

        // Plan mappings
        CreateMap<CreatePlanRequest, Domain.Entities.SuperAdmin.Plan>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Features, opt => opt.Ignore())
            .ForMember(dest => dest.Limits, opt => opt.Ignore())
            .ForMember(dest => dest.Modules, opt => opt.Ignore());

        CreateMap<CreatePlanCommand, CreatePlanRequest>()
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
            .ForMember(dest => dest.Limits, opt => opt.MapFrom(src => src.Limits))
            .ForMember(dest => dest.Modules, opt => opt.MapFrom(src => src.Modules));

        CreateMap<CreatePlanFeatureCommand, CreatePlanFeatureRequest>();
        CreateMap<CreatePlanLimitsCommand, CreatePlanLimitsRequest>();

        // Additional mappings for service layer
        CreateMap<CreatePlanFeatureRequest, Domain.Entities.SuperAdmin.PlanFeature>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PlanId, opt => opt.Ignore())
            .ForMember(dest => dest.Plan, opt => opt.Ignore());

        CreateMap<CreatePlanLimitsRequest, Domain.Entities.SuperAdmin.PlanLimits>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PlanId, opt => opt.Ignore())
            .ForMember(dest => dest.Plan, opt => opt.Ignore());

        // Plan response mappings
        CreateMap<Domain.Entities.SuperAdmin.Plan, PlanResponseDto>()
            .ForMember(dest => dest.Modules, opt => opt.Ignore())
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
            .ForMember(dest => dest.Limits, opt => opt.MapFrom(src => src.Limits));

        CreateMap<Domain.Entities.SuperAdmin.PlanFeature, PlanFeatureResponseDto>();
        CreateMap<Domain.Entities.SuperAdmin.PlanLimits, PlanLimitsResponseDto>();

        // Update plan mappings
        CreateMap<UpdatePlanCommand, UpdatePlanRequest>()
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
            .ForMember(dest => dest.Limits, opt => opt.MapFrom(src => src.Limits))
            .ForMember(dest => dest.Modules, opt => opt.MapFrom(src => src.Modules));

        CreateMap<UpdatePlanFeatureCommand, UpdatePlanFeatureRequest>();
        CreateMap<UpdatePlanLimitsCommand, UpdatePlanLimitsRequest>();

        // Additional mappings for service layer
        CreateMap<UpdatePlanFeatureRequest, Domain.Entities.SuperAdmin.PlanFeature>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PlanId, opt => opt.Ignore())
            .ForMember(dest => dest.Plan, opt => opt.Ignore());

        CreateMap<UpdatePlanLimitsRequest, Domain.Entities.SuperAdmin.PlanLimits>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PlanId, opt => opt.Ignore())
            .ForMember(dest => dest.Plan, opt => opt.Ignore());

        // Module mappings
        CreateMap<CreateModuleRequest, Domain.Entities.SuperAdmin.Module>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Features, opt => opt.Ignore())
            .ForMember(dest => dest.Pricing, opt => opt.Ignore())
            .ForMember(dest => dest.Dependencies, opt => opt.Ignore());

        CreateMap<CreateModuleCommand, CreateModuleRequest>()
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
            .ForMember(dest => dest.Pricing, opt => opt.MapFrom(src => src.Pricing))
            .ForMember(dest => dest.Dependencies, opt => opt.MapFrom(src => src.Dependencies));

        CreateMap<CreateModuleFeatureCommand, CreateModuleFeatureRequest>();
        CreateMap<CreateModulePricingCommand, CreateModulePricingRequest>();

        // Additional mappings for service layer
        CreateMap<CreateModuleFeatureRequest, Domain.Entities.SuperAdmin.ModuleFeature>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ModuleId, opt => opt.Ignore())
            .ForMember(dest => dest.Module, opt => opt.Ignore())
            .ForMember(dest => dest.Configuration, opt => opt.Ignore());

        CreateMap<CreateModulePricingRequest, Domain.Entities.SuperAdmin.ModulePricing>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ModuleId, opt => opt.Ignore())
            .ForMember(dest => dest.Module, opt => opt.Ignore());

        // Update module mappings
        CreateMap<UpdateModuleCommand, UpdateModuleRequest>()
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
            .ForMember(dest => dest.Pricing, opt => opt.MapFrom(src => src.Pricing))
            .ForMember(dest => dest.Dependencies, opt => opt.MapFrom(src => src.Dependencies));

        CreateMap<UpdateModuleFeatureCommand, UpdateModuleFeatureRequest>();
        CreateMap<UpdateModulePricingCommand, UpdateModulePricingRequest>();

        // Additional mappings for service layer
        CreateMap<UpdateModuleFeatureRequest, Domain.Entities.SuperAdmin.ModuleFeature>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ModuleId, opt => opt.Ignore())
            .ForMember(dest => dest.Module, opt => opt.Ignore())
            .ForMember(dest => dest.Configuration, opt => opt.Ignore());

        CreateMap<UpdateModulePricingRequest, Domain.Entities.SuperAdmin.ModulePricing>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ModuleId, opt => opt.Ignore())
            .ForMember(dest => dest.Module, opt => opt.Ignore());

        // Module response mappings
        CreateMap<Domain.Entities.SuperAdmin.Module, ModuleResponseDto>()
            .ForMember(dest => dest.Dependencies, opt => opt.Ignore())
            .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
            .ForMember(dest => dest.Pricing, opt => opt.MapFrom(src => src.Pricing));

        CreateMap<Domain.Entities.SuperAdmin.ModuleFeature, ModuleFeatureResponseDto>()
            .ForMember(dest => dest.Configuration, opt => opt.Ignore());

        CreateMap<Domain.Entities.SuperAdmin.ModulePricing, ModulePricingResponseDto>();
    }
}
