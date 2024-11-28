using Falcon.Client.Utils;
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
        public event Action OnConnected;
        public event Action OnDisconnected;

        private void CreateHub(string token)
        {
            ConfigHelper.CreateConfig();
            var config = ConfigHelper.GetConfig();
            connection = new HubConnectionBuilder()
                .WithUrl($"{config.ConnectionString}chathub?access_token=" + token, options =>
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
            connection.On("OnConnected", () => OnConnected?.Invoke());
            connection.On("OnDisconnected", () => OnDisconnected?.Invoke());
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