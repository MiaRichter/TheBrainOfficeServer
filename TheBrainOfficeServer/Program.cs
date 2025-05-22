using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Globalization;
using TheBrainOfficeServer.Services;
using TheBrainOfficeServer.Repositories;

namespace TheBrainOfficeServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Конфигурация
            builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;
                config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            });

            // Сервисы
            builder.Services.AddControllers();

            // База данных и репозитории
            builder.Services.AddSingleton(provider =>
                new AppDBService(builder.Configuration.GetConnectionString("PostgreSQL")));

            builder.Services.AddSingleton<InitializeRepo>();

            // CORS (разрешаем все для разработки)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Конфигурация middleware
            var cultureInfo = new CultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseCors("AllowAll");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}