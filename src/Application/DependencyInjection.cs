using System.Reflection;
using HotelManagement.Application.Common.Behaviours;
using HotelManagement.Application.Common.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManagement.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(typeof(AdministratorMappingProfile).Assembly);
        services.AddAutoMapper(typeof(AuthMappingProfile).Assembly);

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddHttpContextAccessor();

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
