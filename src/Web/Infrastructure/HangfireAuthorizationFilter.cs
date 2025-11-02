using Hangfire.Dashboard;

namespace HotelManagement.Web.Infrastructure;

/// <summary>
/// Authorization filter for Hangfire dashboard
/// Only allows access to authenticated SuperAdmin users
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Allow access only to authenticated SuperAdmin users
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole("SuperAdmin");
    }
}

