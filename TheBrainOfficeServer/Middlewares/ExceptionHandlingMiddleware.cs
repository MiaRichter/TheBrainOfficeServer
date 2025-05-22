using System.Text.Json;
using TheBrainOfficeServer.Exceptions;

namespace TheBrainOfficeServer.Middlewares
{
    public class ExceptionHandlingMiddleware(RequestDelegate next)
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
            context.Response.ContentType = "application/json";

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

            context.Response.StatusCode = 505;

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
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