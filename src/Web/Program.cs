using Hangfire;
using Hellang.Middleware.ProblemDetails;
using HotelManagement.Application;
using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Infrastructure;
using HotelManagement.Infrastructure.Data;
using HotelManagement.ServiceDefaults;
using HotelManagement.Web;
using HotelManagement.Web.Middlewares;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;
    options.EnablePerformanceCounterCollectionModule = true;
    options.EnableDependencyTrackingTelemetryModule = true;
    options.EnableQuickPulseMetricStream = true;
});

builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");
builder.AddRedisClient("cache");

var cacheConnectionString = builder.Configuration.GetConnectionString("cache");
if (!string.IsNullOrEmpty(cacheConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = cacheConnectionString;
        options.InstanceName = "HotelManagement";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddKeyVaultIfConfigured(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices();
builder.Services.AddHttpContextAccessor();

// Configure request size limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 104857600; // 100MB
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.AddSuperAdminAuthentication(builder.Configuration);
builder.Services.AddSuperAdminAuthorization();

// Configure Hangfire for background job processing
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("sql"), new Hangfire.SqlServer.SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Add Hangfire Server
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = builder.Configuration.GetValue<int>("Hangfire:WorkerCount", 5);
});

builder.Services.AddProblemDetails(options =>
{
    options.IncludeExceptionDetails = (ctx, ex) => builder.Environment.IsDevelopment();

    options.Map<ValidationException>(ex => new ProblemDetails
    {
        Title = "Validation Failed",
        Status = StatusCodes.Status400BadRequest,
        Detail = ex.Message,
        Extensions = { ["errors"] = ex.Errors }
    });

    options.Map<UnauthorizedAccessException>(ex => new ProblemDetails
    {
        Title = "Unauthorized",
        Status = StatusCodes.Status401Unauthorized,
        Detail = ex.Message,
    });

    options.Map<KeyNotFoundException>(ex => new ProblemDetails
    {
        Title = "Not Found",
        Status = StatusCodes.Status404NotFound,
        Detail = ex.Message
    });

    options.Map<Microsoft.AspNetCore.Http.BadHttpRequestException>(ex => new ProblemDetails
    {
        Title = "Bad Request",
        Status = StatusCodes.Status400BadRequest,
        Detail = ex.Message
    });

    options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
});

var app = builder.Build();

// Apply database migrations
if (app.Environment.IsDevelopment())
{
    await DatabaseMigrationService.MigrateDatabaseAsync(app.Services, app.Environment);
}
else
{
    app.UseHsts();

    // In production, only check for pending migrations and warn
    var dbInfo = await DatabaseMigrationService.GetDatabaseInfoAsync(app.Services);
    if (dbInfo.HasPendingMigrations)
    {
        app.Logger.LogWarning(
            "⚠️ Database has {Count} pending migration(s). Please apply migrations before deployment.",
            dbInfo.PendingMigrations.Count);
    }
}


app.UseExceptionHandler("/error");
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<CorsMiddleware>();
app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseOpenApi();
app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/swagger/v1/swagger.json";
});

app.UseRouting();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseMiddleware<TenantRateLimitingMiddleware>();
app.UseMiddleware<SuperAdminMiddleware>();
app.UseMiddleware<IdempotencyMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});
app.UseMiddleware<AuthorizationFailureMiddleware>();

app.UseAntiforgery();

app.UseOutputCache();

app.MapControllers();

// SignalR hubs
app.MapHub<HotelManagement.Web.Hubs.NotificationHub>("/hubs/notifications");

// Health check endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                data = e.Value.Data
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { status = report.Status.ToString() });
    }
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false // Just check if app is running
});

app.Run();

public partial class Program { }
