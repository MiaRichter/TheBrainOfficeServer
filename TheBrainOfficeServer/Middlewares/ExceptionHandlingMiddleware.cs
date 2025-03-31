using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using TheBrainOfficeServer.Exceptions;

namespace TheBrainOfficeServer.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(exception, "Global exception caught");

            context.Response.ContentType = "application/json";

            // Используем разные имена переменных для разных случаев
            var userMessage = exception is AppException appException
                ? appException.UserMessage
                : "Произошла непредвиденная ошибка";

            var debugInfo = exception is AppException appEx
                ? appEx.DebugContext
                : exception.Message;

            var response = new
            {
                UserMessage = userMessage,
                DebugInfo = debugInfo,
                StackTrace = context.Request.IsLocal()
                    ? exception.StackTrace
                    : null
            };

            context.Response.StatusCode = exception switch
            {
                AppException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }

    public static class RequestExtensions
    {
        public static bool IsLocal(this HttpRequest request)
        {
            return request.Host.Host.Contains("localhost") ||
                   request.Host.Host.Contains("127.0.0.1");
        }
    }
}