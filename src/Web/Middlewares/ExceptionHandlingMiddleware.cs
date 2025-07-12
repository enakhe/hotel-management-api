using System.Net;
using HotelManagement.Application.Common.Exceptions;

namespace HotelManagement.Web.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env)
{
    private readonly RequestDelegate _next = next;
    private readonly IWebHostEnvironment _env = env;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = context.Response.StatusCode,
                message = "Validation failed",
                errors = ex.Errors
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = 404,
                message = ex.Message
            });
        }
        catch (HotelManagement.Application.Common.Exceptions.NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = 404,
                message = ex.Message
            });
        }
        catch (ForbiddenAccessException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = context.Response.StatusCode,
                message = ex.Message
            });
        }
        catch (ArgumentNullException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = context.Response.StatusCode,
                message = ex.Message
            });
        }
        catch (ConflictException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await context.Response.WriteAsJsonAsync(new
            {
                statusCode = context.Response.StatusCode,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                statusCode = context.Response.StatusCode,
                message = "An unexpected error occurred",
                detail = _env.IsDevelopment() ? ex.ToString() : ex.Message
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
