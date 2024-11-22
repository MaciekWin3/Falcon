using Falcon.Client.Features.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Falcon.Client.Features.Chat
{
    public class ChatService : IChatService
    {
        private readonly IConfiguration configuration;
        private readonly SignalRClient signalRClient;
        private HubConnection connection;

        public ChatService(IConfiguration configuration, SignalRClient signalRClient)
        {
            this.configuration = configuration;
            this.signalRClient = signalRClient;
        }

        public async Task RunAsync(string token)
        {
            if (token.Length != 0)
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
            }
            else
            {
                // todo: handle it
                Environment.Exit(0);
            }
            await connection.StartAsync();
        }

        public async Task<IList<string>> GetListOfRoomAsync()
        {
            return await signalRClient.connection.InvokeAsync<IList<string>>("ShowActiveRooms");
        }

        public async Task SendDirectMessageAsync(string message)
        {
            List<string> splitString = message.Split(' ').ToList();
            int index = message.IndexOf(' ', message.IndexOf(' ') + 1);
            if (splitString[0] == "/dm")
            {
                await connection.InvokeCoreAsync("SendDirectMessage", args: new[] { message.Remove(0, index), splitString[1] });
            }
        }

        public async Task QuitRoomAsync()
        {
            await connection.InvokeAsync("QuitRoom");
        }

        public async Task<string> GetUsernameAsync()
        {
            return await connection.InvokeAsync<string>("GetUsername");
        }

        public async Task<List<string>> GetUsersAsync()
        {
            var users = await connection.InvokeAsync<List<string>>("ShowUsersInRoom");
            return users;
        }
    }
}