using Hellang.Middleware.ProblemDetails;
using HotelManagement.Application;
using HotelManagement.Application.Common.Exceptions;
using HotelManagement.Infrastructure;
using HotelManagement.Infrastructure.Data;
using HotelManagement.ServiceDefaults;
using HotelManagement.Web;
using HotelManagement.Web.Middlewares;
using HotelManagement.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddSuperAdminAuthentication(builder.Configuration);
builder.Services.AddSuperAdminAuthorization();

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

    options.Map<BadHttpRequestException>(ex => new ProblemDetails
    {
        Title = "Bad Request",
        Status = StatusCodes.Status400BadRequest,
        Detail = ex.Message
    });

    options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    app.UseHsts();
}


app.UseExceptionHandler("/error");
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
app.UseMiddleware<AuthorizationFailureMiddleware>();

app.UseAntiforgery();

app.UseOutputCache();

app.MapControllers();
app.Map("/", () => Results.Redirect("/api"));

app.Run();

public partial class Program { }
