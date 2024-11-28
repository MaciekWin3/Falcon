using Falcon.Client;
using Falcon.Client.Features.Auth;
using Falcon.Client.Features.Auth.UI;
using Falcon.Client.Features.Chat;
using Falcon.Client.Features.Chat.UI;
using Falcon.Client.Features.Lobby.UI;
using Falcon.Client.Features.SignalR;
using Falcon.Client.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

ConfigHelper.CreateConfig();
var config = ConfigHelper.GetConfig();

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
            client.BaseAddress = new Uri(config.ConnectionString);
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

var svc = ActivatorUtilities.CreateInstance<TerminalOrchestrator>(host.Services);
svc.InitApp();
