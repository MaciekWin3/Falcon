using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Falcon.Client.Services
{
    public class ChatService : IChatService
    {
        private static readonly object bufferLock = new();
        private static readonly int windowHeight = Console.BufferHeight;
        private readonly IAuthService authService;

        private readonly List<string> Commands = new()
        {
            "/quit",
            "/active",
            "/chart"
        };

        public ChatService(IAuthService authService)
        {
            this.authService = authService;
        }

        public async Task RunAsync()
        {
            // TODO: move connection to ctor
            string token = await authService.Login();
            Console.Clear(); // Check it
            if (token.Length != 0)
            {
                var connection = new HubConnectionBuilder()
                   .WithUrl($"https://localhost:7262/chathub?access_token=" + token)
                   .ConfigureLogging(configureLogging =>
                   {
                       configureLogging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                       configureLogging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                   })
                   .WithAutomaticReconnect() // Handle it
                   .Build();

                connection.StartAsync().Wait();

                connection.InvokeAsync("SendActiveRooms").Wait();
                var roomsLocal = new List<string>()
                {
                    "All",
                    "Maciek",
                    "dotnet"
                };
                var room = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Choose room: ")
                            .PageSize(10)
                            .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
                            .AddChoices(roomsLocal));

                // TODO
                //await connection.InvokeAsync("ConnectToRoom");
                await connection.InvokeCoreAsync("JoinRoom", args: new[] { "Maciek" });
                //connection.On("JoinRoom", (string room) =>
                //{
                //});

                connection.On("ReceiveMessage", (string userName, string message) =>
                {
                    lock (bufferLock)
                    {
                        // Need changes
                        Console.CursorVisible = false;
                        Console.SetCursorPosition(0, windowHeight - 1);
                        try
                        {
                            AnsiConsole.MarkupLine
                                ($"[blue]{userName}[/][yellow] <{DateTime.Now:HH:mm:ss}>[/]: [green]{message}[/]"); // Change for deafult console
                        }
                        catch
                        {
                            // Needs to be beter, for example coloring
                            Console.WriteLine($"{userName} <{DateTime.Now:HH:mm:ss}>[/]: {message}");
                        }
                        Console.SetCursorPosition(0, windowHeight - 1);
                        AnsiConsole.Markup($"[blue]You[/][yellow]<{DateTime.Now:HH: mm:ss}>[/]: ");
                        Console.CursorVisible = true;
                    }
                });

                string message = string.Empty;

                do
                {
                    lock (bufferLock)
                    {
                        Console.SetCursorPosition(0, windowHeight - 1);
                        Console.Write("Message: ");
                        Console.CursorVisible = true;
                    }

                    message = Console.ReadLine();
                    if (message.Length != 0) // Disables option to send empty messages
                    {
                        if (message is not null || message![0] != '/')
                        {
                            lock (bufferLock)
                            {
                                connection.InvokeCoreAsync("SendGroupMessageAsync", args: new[] { message });
                                Console.CursorVisible = false;
                            }
                        }
                        if (message[0] == '/' && Commands.Contains(message))
                        {
                            ExecuteCommand();
                        }
                    }
                } while (!string.Equals(message, "/quit", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                Console.WriteLine("Application stopped working!");
            }
        }

        public void ExecuteCommand()
        {
            throw new NotImplementedException();
        }
    }
}