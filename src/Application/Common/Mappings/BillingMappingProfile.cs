using AutoMapper;
using HotelManagement.Application.Common.DTOs;
using HotelManagement.Domain.Entities;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.Mappings;

public class BillingMappingProfile : Profile
{
    public BillingMappingProfile()
    {
        // Subscription mappings
        CreateMap<Subscription, SubscriptionResponseDto>()
            .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.Name))
            .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Plan.Name))
            .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.BillingCycleDisplay, opt => opt.MapFrom(src => src.BillingCycle.ToString()))
            .ForMember(dest => dest.Modules, opt => opt.MapFrom(src => src.SubscriptionModules.Where(sm => !sm.RemovedAt.HasValue)));

        CreateMap<SubscriptionModule, SubscriptionModuleDto>()
            .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module != null ? src.Module.Name : string.Empty));

        // Invoice mappings
        CreateMap<Invoice, InvoiceResponseDto>()
            .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.Name))
            .ForMember(dest => dest.SubscriptionNumber, opt => opt.MapFrom(src => src.Subscription.SubscriptionNumber))
            .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TypeDisplay, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => 
                src.Status != InvoiceStatus.Paid && src.DueDate < DateTime.UtcNow))
            .ForMember(dest => dest.DaysOverdue, opt => opt.MapFrom(src =>
                src.Status != InvoiceStatus.Paid && src.DueDate < DateTime.UtcNow 
                    ? (DateTime.UtcNow - src.DueDate).Days 
                    : 0));

        CreateMap<InvoiceLineItem, InvoiceLineItemDto>()
            .ForMember(dest => dest.ModuleName, opt => opt.MapFrom(src => src.Module != null ? src.Module.Name : null))
            .ForMember(dest => dest.TypeDisplay, opt => opt.MapFrom(src => src.Type.ToString()));

        // Payment mappings
        CreateMap<Payment, PaymentResponseDto>()
            .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.Invoice.InvoiceNumber))
            .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.Name))
            .ForMember(dest => dest.MethodDisplay, opt => opt.MapFrom(src => src.Method.ToString()))
            .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.GatewayDisplay, opt => opt.MapFrom(src => src.Gateway.ToString()));

        // Usage tracking mappings
        CreateMap<UsageAggregation, UsageAggregationDto>();
    }
}

