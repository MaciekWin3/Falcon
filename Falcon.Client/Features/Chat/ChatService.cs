using Falcon.Client.Features.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Falcon.Client.Features.Chat
{
    public class ChatService
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

        public async Task CheckForCommandAsync(string message)
        {
            if (!message.StartsWith('/'))
            {
                return;
            }
            List<string> splitString = message.Split(' ').ToList();
            int index = message.IndexOf(' ', message.IndexOf(' ') + 1);
            if (splitString[0] == "/dm")
            {
                await connection.InvokeCoreAsync("SendDirectMessage", args: new[] { message.Remove(0, index), splitString[1] });
            }
            else
            {
                switch (message)
                {
                    // Check if this is working
                    case "/quit":
                        await connection.InvokeAsync("QuitRoom");
                        //await ChooseRoom();
                        break;

                    case "/users":
                        // Needs improvments and move to own function
                        var users = await connection.InvokeAsync<List<string>>("ShowUsersInRoom");
                        var table = new Table();
                        table.AddColumn("Users");
                        foreach (var user in users)
                        {
                            table.AddRow($"[green]{user}[/]");
                        }
                        AnsiConsole.Write(table);
                        break;

                    case "/exit":
                        AnsiConsole.MarkupLine("[green]Exiting application...[/]");
                        break;

                    default:
                        AnsiConsole.MarkupLine("[red]Invalid command![/]");
                        break;
                }
            }
        }
    }
}