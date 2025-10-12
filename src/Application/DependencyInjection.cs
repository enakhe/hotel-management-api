using System.Reflection;
using HotelManagement.Application.Common.Behaviours;
using HotelManagement.Application.Common.Mappings;
using HotelManagement.Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.Tenant;

namespace HotelManagement.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(typeof(AdministratorMappingProfile).Assembly);
        services.AddAutoMapper(typeof(SuperAdministratorMappingProfile).Assembly);
        services.AddAutoMapper(typeof(AuthMappingProfile).Assembly);

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddHttpContextAccessor();

        // Register tenant services
        services.AddScoped<ITenantContext, TenantContext>();

        // Register SuperAdmin services
        services.AddScoped<ISuperAdminContext, SuperAdminContext>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });

        return services;
    }
}
