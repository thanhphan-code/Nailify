using System.Net;
using System.Text.Json;
using Nailify.Application.Common.Exceptions;

namespace Nailify.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, body) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                (object)new
                {
                    title = "Validation failed",
                    errors = validationEx.Errors
                }),
            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                (object)new { title = conflictEx.Message }),
            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                (object)new { title = unauthorizedEx.Message }),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                (object)new { title = "Authentication is required." }),
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                (object)new { title = notFoundEx.Message }),
            _ => (
                HttpStatusCode.InternalServerError,
                (object)new { title = "An unexpected error occurred." })
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
