using System.Reflection;
using HotelManagement.Application.Common.Behaviours;
using HotelManagement.Application.Common.Interfaces.Administrator;
using HotelManagement.Application.Common.Interfaces.Auth;
using HotelManagement.Application.Common.Mappings;
using HotelManagement.Application.Common.Services.Administrator;
using HotelManagement.Application.Common.Services.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManagement.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(typeof(AdministratorMappingProfile).Assembly);
        services.AddAutoMapper(typeof(AuthMappingProfile).Assembly);

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IAuthService, AuthService>();

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
