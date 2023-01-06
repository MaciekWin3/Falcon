using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Falcon.Client.Features.SignalR
{
    public class SignalRClient : ISignalRClient
    {
        public HubConnection connection;
        private readonly IConfiguration configuration;

        public SignalRClient(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public event Action<string, string> OnReceiveMessage;

        public event Action<string> OnConnect;

        public event Action<string> OnDisconnect;

        private void CreateHub(string token)
        {
            connection = new HubConnectionBuilder()
                .WithUrl($"{configuration["ServerIp"]}chathub?access_token=" + token, options =>
                {
                    options.AccessTokenProvider = () =>
                    {
                        return Task.FromResult(token);
                    };
                })
                .ConfigureLogging(configureLogging =>
                {
                    configureLogging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                    configureLogging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                })
                .WithAutomaticReconnect() // Handle it
                .Build();

            connection.On<string, string>("ReceiveMessage", (userName, message) => OnReceiveMessage?.Invoke(userName, message));
            connection.On<string>("Connected", (username) => OnConnect?.Invoke(username));
            connection.On<string>("Disconnected", (username) => OnDisconnect?.Invoke(username));
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