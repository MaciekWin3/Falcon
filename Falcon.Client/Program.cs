using Falcon.Client.Interfaces;
using Falcon.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Falcon.Client
{
    internal class Program
    {
        private static async Task Main()
        {
            var builder = new ConfigurationBuilder();
            BuildConfig(builder);
            var configuration = builder.Build();

            // Disable logging
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<IAuthService, AuthService>();
                    services.AddTransient<IChatService, ChatService>();
                    services.AddTransient<IFalconOrchestratorService, FalconOrchestratorService>();
                    services.AddHttpClient("Server", client =>
                    {
                        client.BaseAddress = new Uri(configuration["ServerIp"]);
                    });
                    services.AddLogging(builder =>
                        {
                            builder
                                .AddFilter("Microsoft", LogLevel.Warning)
                                .AddFilter("System", LogLevel.Warning)
                                .AddFilter("NToastNotify", LogLevel.Warning)
                                .AddConsole();
                        }
                    );
                })
                .Build();

            var svc = ActivatorUtilities.CreateInstance<FalconOrchestratorService>(host.Services);
            await svc.DisplayMenu();
        }

        private static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json")
                .AddEnvironmentVariables();
        }
    }
}