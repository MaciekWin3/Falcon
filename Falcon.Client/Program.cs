using Falcon.Client.Features.Auth;
using Falcon.Client.Features.Auth.UI;
using Falcon.Client.Features.Chat;
using Falcon.Client.Features.Chat.UI;
using Falcon.Client.Features.Lobby.UI;
using Falcon.Client.Features.SignalR;
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

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Services
                    services.AddScoped<SignalRClient>();
                    services.AddTransient<ChatService>();
                    services.AddTransient<AuthService>();
                    services.AddTransient<TerminalOrchestrator>();

                    // Windows
                    services.AddTransient<ChatWindow>();
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<LobbyWindow>();

                    services.AddHttpClient("Server", client =>
                    {
                        client.BaseAddress = new Uri(configuration["ServerIp"]);
                    });
                    // Do we need this?
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

            var svc = ActivatorUtilities.CreateInstance<TerminalOrchestrator>(host.Services);
            // Added await
            await svc.Run();
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