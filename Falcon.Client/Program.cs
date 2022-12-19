﻿using Falcon.Client.Interfaces;
using Falcon.Client.Services;
using Falcon.Client.Terminal;
using Falcon.Client.Windows;
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
                    // Obsolete Services
                    services.AddTransient<IAuthService, AuthService>();
                    services.AddTransient<IChatService, ChatService>();
                    services.AddTransient<IFalconOrchestratorService, FalconOrchestratorService>();

                    // Services
                    services.AddScoped<SignalRClient>();
                    services.AddTransient<ChatService2>();
                    services.AddTransient<AuthService2>();
                    services.AddTransient<ITerminalOrchestrator, TerminalOrchestrator>();

                    // Windows
                    services.AddTransient<ChatWindow>();
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<RoomWindow>();

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