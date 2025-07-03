using System.Net;
using System.Text.Json;
using ManaxApi.Models;

namespace ManaxApi.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger,
    IWebHostEnvironment env)
{
    public async System.Threading.Tasks.Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async System.Threading.Tasks.Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogError(exception, "An unhandled error occurred: {Message}", exception.Message);

        string traceId = Guid.NewGuid().ToString();

        logger.LogError(exception,
            "Detailed exception for TraceId: {TraceId}, Message: {Message}, Stack: {StackTrace}",
            traceId, exception.Message, exception.StackTrace);

        ApiError error = new()
        {
            Status = (int)HttpStatusCode.InternalServerError,
            TraceId = traceId,
            Message = env.IsDevelopment()
                ? exception.Message
                : "An internal server error occurred. Please contact the administrator with the provided trace ID."
        };

        switch (exception)
        {
            case UnauthorizedAccessException:
                error.Status = (int)HttpStatusCode.Unauthorized;
                error.Message = "You are not authorized to perform this operation.";
                break;
            case KeyNotFoundException:
                error.Status = (int)HttpStatusCode.NotFound;
                error.Message = "The requested resource was not found.";
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = error.Status;

        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
    }
}