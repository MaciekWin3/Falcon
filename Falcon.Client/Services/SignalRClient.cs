using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Falcon.Client.Services
{
    public class SignalRClient
    {
        public HubConnection connection;
        private readonly IConfiguration configuration;

        public SignalRClient(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public event Action<string, string> OnReceiveMessage;

        private void CreateHub(string token)
        {
            connection = new HubConnectionBuilder()
                .WithUrl($"{configuration["ServerIp"]}chathub?access_token=" + token)
                .ConfigureLogging(configureLogging =>
                {
                    configureLogging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                    configureLogging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                })
                .WithAutomaticReconnect() // Handle it
                .Build();

            connection.On<string, string>("ReceiveMessage", (userName, message) => OnReceiveMessage?.Invoke(userName, message));
        }

        public async Task StartConnectionAsync(string token)
        {
            try
            {
                CreateHub(token);
            }
            catch (Exception)
            {
            }
            await connection.StartAsync();
        }
    }
}