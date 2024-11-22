using Falcon.Client;
using Falcon.Client.Features.Auth;
using Falcon.Client.Features.Auth.UI;
using Falcon.Client.Features.Chat;
using Falcon.Client.Features.Chat.UI;
using Falcon.Client.Features.Lobby.UI;
using Falcon.Client.Features.SignalR;
using Falcon.Client.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = new ConfigurationBuilder();
BuildConfig(builder);
var configuration = builder.Build();

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

var svc = ActivatorUtilities.CreateInstance<TerminalOrchestrator>(host.Services);
svc.InitApp();

void BuildConfig(IConfigurationBuilder builder)
{
    builder.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();
}
