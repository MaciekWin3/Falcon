using Falcon.Client.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Falcon.Client.Services
{
    public class ChatService : IChatService
    {
        private static readonly object bufferLock = new();
        private static readonly int windowHeight = Console.BufferHeight;
        private readonly IAuthService authService;
        private readonly IConfiguration configuration;
        private HubConnection connection;

        public ChatService(IAuthService authService, IConfiguration configuration)
        {
            this.authService = authService;
            this.configuration = configuration;
        }

        public async Task RunChat()
        {
            string message;
            do
            {
                lock (bufferLock)
                {
                    // Move to other function???
                    Console.SetCursorPosition(0, windowHeight - 1);
                    AnsiConsole.Markup($"[fuchsia]You[/][yellow]:[/] ");
                    Console.CursorVisible = true;
                }
                message = Console.ReadLine();

                if (!string.IsNullOrEmpty(message))
                {
                    var cursorPosition = Console.GetCursorPosition();
                    ClearCurrentConsoleLine(cursorPosition, cursorPosition.Top - 1);
                    Console.SetCursorPosition(0, cursorPosition.Top - 1);
                    AnsiConsole.Markup($"[fuchsia]You[/][yellow]|{DateTime.Now:HH:mm:ss}|:[/] {message}");
                    Console.SetCursorPosition(cursorPosition.Left, cursorPosition.Top);
                    if (message[0] != '/')
                    {
                        lock (bufferLock)
                        {
                            connection.InvokeCoreAsync("SendGroupMessageAsync", args: new[] { message });
                            Console.CursorVisible = false;
                        }
                    }
                    else
                    {
                        await ExecuteCommand(message);
                    }
                }
            } while (!string.Equals(message, "/exit", StringComparison.OrdinalIgnoreCase));
        }

        public async Task RunAsync()
        {
            // Add Progress Bar???
            string token = await authService.Login();
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
                AnsiConsole.MarkupLine("[red]Unable to connect to the server! Check your connection or try again later![/]");
                Environment.Exit(-1);
            }

            if (connection is not null)
            {
                Console.Clear();
                connection.StartAsync().Wait();
                await ChooseRoom();
                ConnectionListener();
                await RunChat();
            }
        }

        protected void ConnectionListener()
        {
            connection.On("ReceiveMessage", (string userName, string message) =>
            {
                lock (bufferLock)
                {
                    // Need changes
                    Console.CursorVisible = false;
                    WriteLineMultithread(userName, message);
                    Console.CursorVisible = true;
                }
            });
        }

        protected async Task ChooseRoom()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            var rooms = await connection.InvokeAsync<IList<string>>("ShowActiveRooms");

            var room = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose room: ")
                        .PageSize(10)
                        .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
                        .AddChoices(rooms));

            if (room == "Create new room")
            {
                await CreateNewRoom();
                return;
            }

            await connection.InvokeCoreAsync("JoinRoom", args: new[] { room });
        }

        protected async Task CreateNewRoom()
        {
            // Need more work
            Console.Clear();
            var roomName = AnsiConsole.Ask<string>("Room name: ");
            var created = await connection.InvokeCoreAsync<bool>("CreateRoom", args: new[] { roomName });
            if (created)
            {
                await ChooseRoom();
            }
            else
            {
                Environment.Exit(-1);
            }
        }

        private async Task ExecuteCommand(string command)
        {
            List<string> splitString = command.Split(' ').ToList();
            int index = command.IndexOf(' ', command.IndexOf(' ') + 1);
            if (splitString[0] == "/dm")
            {
                await connection.InvokeCoreAsync("SendDirectMessage", args: new[] { command.Remove(0, index), splitString[1] });
            }
            else
            {
                switch (command)
                {
                    // Check if this is working
                    case "/quit":
                        await connection.InvokeAsync("QuitRoom");
                        await ChooseRoom();
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

                    default:
                        AnsiConsole.MarkupLine("[red]Invalid command![/]");
                        break;
                }
            }
        }

        public static void ClearCurrentConsoleLine((int, int) currentPosition, int lineNumber)
        {
            Console.SetCursorPosition(0, lineNumber);
            Console.Write(new string(' ', Console.WindowHeight));
            Console.SetCursorPosition(currentPosition.Item1, currentPosition.Item2); //Named tuple?
        }

        public static void WriteLineMultithread(string userName, string message)
        {
            // Add additional checks for line wrapping
            int lastx = Console.CursorLeft;
            Console.WriteLine();
            int lasty = Console.BufferHeight - 1;
            // Source width handle mulitline?
            Console.SetCursorPosition(0, lasty);
#pragma warning disable CA1416 // Validate platform compatibility
            Console.MoveBufferArea(0, lasty - 1, lastx, 1, 0, lasty, ' ', Console.ForegroundColor, Console.BackgroundColor);
#pragma warning restore CA1416 // Validate platform compatibility
            Console.SetCursorPosition(0, lasty - 1);
            try
            {
                AnsiConsole.MarkupLine
                   ($"[lime]{userName}[/][yellow]|{DateTime.Now:HH:mm:ss}|:[/] [white]{message}[/]");
            }
            catch
            {
                Console.WriteLine($"{userName}|{DateTime.Now:HH:mm:ss}|:{message}");
            }
            Console.SetCursorPosition(lastx, lasty);
        }
    }
}