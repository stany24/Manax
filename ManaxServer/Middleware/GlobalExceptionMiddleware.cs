using System.Net;
using System.Text.Json;
using ManaxLibrary.Logging;
using ManaxServer.Models;

namespace ManaxServer.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        string traceId = Guid.NewGuid().ToString();

        Logger.LogError("An unhandled error occurred " + traceId + ": ", exception);

        ApiError error = new()
        {
            Status = (int)HttpStatusCode.InternalServerError,
            TraceId = traceId,
            Message = env.IsDevelopment()
                ? exception.Message
                : "An internal server error occurred. Please contact the administrator with the provided trace ID."
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = error.Status;

        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
    }
}