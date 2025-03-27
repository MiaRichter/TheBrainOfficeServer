using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Globalization;
using System.Text;
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
            builder.Configuration
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

            // Логгирование
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Сервисы
            builder.Services.AddControllers();

            // База данных и репозитории
            var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
            builder.Services.AddSingleton<AppDBService>(new AppDBService(connectionString));
            builder.Services.AddScoped<ComponentRepo>();

           
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
            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}