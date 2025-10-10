using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HotelManagement.Web.Infrastructure;

/// <summary>
/// Extensions for SuperAdmin authentication and authorization
/// </summary>
public static class SuperAdminAuthenticationExtensions
{
    /// <summary>
    /// Adds SuperAdmin authentication with separate issuer/audience
    /// </summary>
    public static IServiceCollection AddSuperAdminAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var superAdminJwtSettings = configuration.GetSection("SuperAdminJwt");
        var issuer = superAdminJwtSettings["Issuer"] ?? "https://superadmin.hotelmanagement.com";
        var audience = superAdminJwtSettings["Audience"] ?? "superadmin-api";
        var secretKey = superAdminJwtSettings["SecretKey"] ?? "SuperSecretKeyForSuperAdminAuthentication123456789";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer("SuperAdmin", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = async context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }

                    // Return proper JSON error response for authentication failures
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";

                    var errorResponse = new
                    {
                        statusCode = 401,
                        title = "Unauthorized",
                        message = "SuperAdmin authentication failed. Please provide a valid SuperAdmin token.",
                        type = "https://tools.ietf.org/html/rfc7235#section-3.1"
                    };

                    await context.Response.WriteAsJsonAsync(errorResponse);
                },
                OnChallenge = async context =>
                {
                    // Handle challenge (when no token is provided)
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";

                    var errorResponse = new
                    {
                        statusCode = 401,
                        title = "Unauthorized",
                        message = "SuperAdmin authentication required. Please provide a valid SuperAdmin token.",
                        type = "https://tools.ietf.org/html/rfc7235#section-3.1"
                    };

                    await context.Response.WriteAsJsonAsync(errorResponse);
                }
            };
        });

        return services;
    }

    /// <summary>
    /// Adds SuperAdmin authorization policies
    /// </summary>
    public static IServiceCollection AddSuperAdminAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("SuperAdmin", policy =>
            {
                policy.RequireRole("SuperAdmin");
                policy.RequireClaim("superadmin", "true");
            })
            .AddPolicy("RequireStepUpAuth", policy =>
            {
                policy.RequireRole("SuperAdmin");
                policy.RequireClaim("superadmin", "true");
                policy.RequireClaim("step_up_auth", "true");
                policy.RequireClaim("mfa_verified", "true");
            })
            .AddPolicy("RequireTwoManRule", policy =>
            {
                policy.RequireRole("SuperAdmin");
                policy.RequireClaim("superadmin", "true");
                policy.RequireClaim("step_up_auth", "true");
                policy.RequireClaim("mfa_verified", "true");
                policy.RequireClaim("two_man_rule", "true");
            });

        return services;
    }
}
