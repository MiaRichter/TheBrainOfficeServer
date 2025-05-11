using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Iot.Device.Common;
using Iot.Device.DHTxx;
using Microsoft.AspNetCore.WebSockets;
using TheBrainOfficeServer.Middlewares;
using UnitsNet;

namespace TheBrainOfficeServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Конфигурация для Linux
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            // Сервисы
            builder.Services.AddControllers();

            // WebSockets
            builder.Services.AddWebSockets(options =>
            {
                options.KeepAliveInterval = TimeSpan.FromMinutes(2);
            });

            // CORS для Linux
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MobileCors", policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                    {
                        if (string.IsNullOrEmpty(origin)) return true;

                        if (origin.StartsWith("http://10.0.2.2") ||
                            origin.StartsWith("http://192.168.") ||
                            origin.StartsWith("http://172.") ||
                            origin.StartsWith("http://localhost"))
                        {
                            return true;
                        }

                        return false;
                    })
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("*");
                });
            });

            var app = builder.Build();

            // Настройки для Linux
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                                 Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            });

            // Культура
            var cultureInfo = new CultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            // Middleware pipeline
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("LinuxCors");
            app.UseAuthorization();

            // WebSockets
            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2),
                AllowedOrigins = { }
            });

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    var origin = context.Request.Headers["Origin"].ToString();
                    if (!string.IsNullOrEmpty(origin) &&
                        (origin.Contains("android") ||
                         origin.Contains("localhost") ||
                         origin.StartsWith("http://192.168.")))
                    {
                        if (context.WebSockets.IsWebSocketRequest)
                        {
                            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            await HandleWebSocketConnection(webSocket);
                            return;
                        }
                    }
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
                await next(context);
            });

            app.MapControllers();

            // Запуск на Linux
            app.Urls.Add("http://0.0.0.0:5000");
            app.Run();
        }

        private static async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                        continue;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"[Linux] Received: {message}");

                    var response = JsonSerializer.Serialize(new
                    {
                        Message = "Received on Linux",
                        YourMessage = message,
                        Timestamp = DateTime.UtcNow
                    });

                    await webSocket.SendAsync(
                        Encoding.UTF8.GetBytes(response),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Linux] WebSocket error: {ex.Message}");
            }
            finally
            {
                if (webSocket?.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Connection closed",
                        CancellationToken.None);
                }
                webSocket?.Dispose();
            }
        }
    }
}