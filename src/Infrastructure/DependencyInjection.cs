using System.Reflection;
using System.Text;
using FluentValidation;
using HotelManagement.Application.Common.Behaviours;
using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Validators.Administrator;
using HotelManagement.Application.Common.Validators.Auth;
using HotelManagement.Application.Core.ModuleManagement.Commands;
using HotelManagement.Application.Core.ModuleManagement.Queries;
using HotelManagement.Application.Core.PlanManagement.Commands;
using HotelManagement.Application.Core.PlanManagement.Queries;
using HotelManagement.Application.Core.Tenant.Commands;
using HotelManagement.Application.Core.Tenant.Queries;
using HotelManagement.Domain.Constants;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Data;
using HotelManagement.Infrastructure.Data.Interceptors;
using HotelManagement.Infrastructure.Data.Services;
using HotelManagement.Infrastructure.Repository;
using HotelManagement.Infrastructure.Repository.Administrator;
using HotelManagement.Infrastructure.Services;
using HotelManagement.Infrastructure.Services.Reports;
using HotelManagement.Infrastructure.Resilience;
using HotelManagement.Infrastructure.Services.Reports.Exporters;
using HotelManagement.Infrastructure.Services.Reports.Generators;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Polly;
using Polly.Extensions.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace HotelManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {

        var connectionString = configuration.GetConnectionString("sql");
        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, TenantInterceptor>();

        var allowedOrigins = new[] { "http://localhost:3000", "https://localhost:3000" };
        services.AddCors(options => options.AddPolicy("AllowSpecificOrigins", policy => policy
                      .WithOrigins(allowedOrigins)
                      .WithHeaders("Access-Control-Allow-Private-Network", "true", "Content-Type", "Authorization", "X-Requested-With", "Accept", "Origin", "X-Request-Id", "X-Tenant-Id", "X-Tenant-Identifier", "Cache-Control", "Pragma", "X-Admin-Portal", "X-App-Version", "X-Debug-Timestamp", "If-Modified-Since", "If-None-Match")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .SetPreflightMaxAge(TimeSpan.FromSeconds(86400))
                      .AllowCredentials()
                      .SetIsOriginAllowedToAllowWildcardSubdomains()));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddAntiforgery();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<ApplicationDbContextInitialiser>();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["Auth.JWT.AccessToken"];
                        return Task.CompletedTask;
                    }
                };
            });

        // Redis Cache
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var redisConfiguration = configuration.GetConnectionString("cache");
            return ConnectionMultiplexer.Connect(redisConfiguration!);
        });

        services.AddAuthorizationBuilder();

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders()
            .AddApiEndpoints();

        services.AddTransient<IUserEmailStore<ApplicationUser>, UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid, IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>, IdentityUserToken<Guid>, IdentityRoleClaim<Guid>>>();

        services.AddTransient<IUserStore<ApplicationUser>, UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid, IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>, IdentityUserToken<Guid>, IdentityRoleClaim<Guid>>>();

        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();

        // Core Report Services
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IReportGeneratorService, ReportGeneratorService>();
        services.AddScoped<IReportExportService, ReportExportService>();
        services.AddScoped<IReportSchedulerService, ReportSchedulerService>();
        services.AddScoped<IReportSubscriptionService, ReportSubscriptionService>();
        services.AddScoped<IReportTemplateService, ReportTemplateService>();

        // Infrastructure Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();

        // Report Data Generators
        services.AddScoped<SystemOverviewReportGenerator>();
        services.AddScoped<FinancialReportGenerator>();
        services.AddScoped<TenantUsageReportGenerator>();
        services.AddScoped<AuditReportGenerator>();
        services.AddScoped<AnalyticsReportGenerator>();

        // Report Exporters
        services.AddScoped<PdfReportExporter>();
        services.AddScoped<ExcelReportExporter>();
        services.AddScoped<CsvReportExporter>();
        services.AddScoped<JsonReportExporter>();
        services.AddScoped<HtmlReportExporter>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IRoleService, RoleService>();
        
        // Register caching service
        services.AddSingleton<ICacheService, CacheService>();

        // Configure HTTP client factory
        services.AddHttpClient("ResilientClient");
        services.AddSingleton<ResilientHttpClientFactory>();
        
        // Note: To enable Polly resilience policies on HTTP clients, configure them manually:
        // services.AddHttpClient("ResilientClient")
        //     .AddPolicyHandler(...)  // Requires proper Polly configuration

        // Register tenant services
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ILicensingService, LicensingService>();
        services.AddScoped<ITenantRegistryService, TenantRegistryService>();
        services.AddScoped<TenantQueryFilterService>();
        services.AddScoped<TenantAwareDbContextFactory>();

        // Register domain services
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<ILicenseKeyService, Application.Common.Services.LicenseKey.LicenseKeyService>();
        services.AddScoped<ILimitsService, LimitsService>();

        // Register SuperAdmin services
        services.AddScoped<ISuperAdminService, SuperAdminService>();
        services.AddScoped<ISuperAdminAuditService, SuperAdminAuditService>();
        services.AddScoped<ITenantAdminService, TenantAdminService>();

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator));

        // Add Health Checks
        services.AddHealthChecks()
            .AddCheck<HotelManagement.Infrastructure.HealthChecks.DatabaseHealthCheck>(
                "database",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                tags: new[] { "db", "sql", "ready" })
            .AddCheck<HotelManagement.Infrastructure.HealthChecks.RedisHealthCheck>(
                "redis",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
                tags: new[] { "cache", "redis", "ready" })
            .AddCheck<HotelManagement.Infrastructure.HealthChecks.HangfireHealthCheck>(
                "hangfire",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
                tags: new[] { "jobs", "hangfire", "ready" })
            .AddCheck<HotelManagement.Infrastructure.HealthChecks.SystemResourcesHealthCheck>(
                "system",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded,
                tags: new[] { "system", "resources" });

        services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateTenantCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<CreatePlanCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdatePlanCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<DeletePlanCommandValidator>();

        // Module Management Validators
        services.AddValidatorsFromAssemblyContaining<CreateModuleCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateModuleCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<DeleteModuleCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<GetModulesQueryValidator>();
        services.AddValidatorsFromAssemblyContaining<GetModuleByIdQueryValidator>();

        // Plan-Module Relationship Validators
        services.AddValidatorsFromAssemblyContaining<AssignModuleToPlanCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<RemoveModuleFromPlanCommandValidator>();

        // Bulk Operation Validators
        services.AddValidatorsFromAssemblyContaining<BulkUpdatePlansCommandValidator>();

        // Analytics Validators
        services.AddValidatorsFromAssemblyContaining<GetPlanUsageQueryValidator>();
        services.AddValidatorsFromAssemblyContaining<GetModuleUsageQueryValidator>();
        services.AddValidatorsFromAssemblyContaining<GetPlansQueryValidator>();
        services.AddValidatorsFromAssemblyContaining<GetPlanByIdQueryValidator>();
        services.AddValidatorsFromAssemblyContaining<ChangePasswordDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateBranchValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<ResetPasswordRequestDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<GetTenantsQueryValidator>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        // Configure EPPlus license for non-commercial use (EPPlus 8+)
        // Note: EPPlus 8+ requires license configuration. This can be done through:
        // 1. appsettings.json configuration section
        // 2. Environment variable: EPPlus__ExcelPackage__LicenseContext=NonCommercial
        // 3. Code-based configuration (if API allows)
        // For now, this will be configured through app settings or environment variables

        return services;
    }
}
