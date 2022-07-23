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
        private HubConnection connection;

        public static Thread t1;
        public static Thread t2;

        public ChatService(IAuthService authService)
        {
            this.authService = authService;
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
                    AnsiConsole.Markup($"[fuchsia]You[/][yellow]|{DateTime.Now:HH:mm:ss}|:[/] ");
                    Console.CursorVisible = true;
                }
                message = Console.ReadLine();

                // if else for commands
                if (message.Length != 0)
                {
                    if (message is not null && message[0] != '/')
                    {
                        lock (bufferLock)
                        {
                            connection.InvokeCoreAsync("SendGroupMessageAsync", args: new[] { message });
                            Console.CursorVisible = false;
                        }
                    }
                    if (message[0] == '/')
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
                   .WithUrl($"http://192.168.68.102:5262/chathub?access_token=" + token)
                   //.WithUrl($"https://localhost:7262/chathub?access_token=" + token)
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
                Console.WriteLine("Application stopped working!");
            }
            Console.Clear(); // Check it

            connection.StartAsync().Wait();

            await ChooseRoom();
            // MultiThreading
            t1 = new Thread(async () => await RunChat());
            t2 = new Thread(async () => await ConnectionListener());
            t1.Start();
            t2.Start();
        }

        protected async Task ConnectionListener()
        {
            connection.On("ReceiveMessage", (string userName, string message) =>
            {
                lock (bufferLock)
                {
                    // Need changes
                    Console.CursorVisible = false;
                    //Console.SetCursorPosition(0, windowHeight - 1);
                    try
                    {
                        WriteLineMultithread($"{userName}|{DateTime.Now:HH:mm:ss}|:{message}");
                        //AnsiConsole.MarkupLine
                        //    ($"[lime]{userName}[/][yellow]|{DateTime.Now:HH:mm:ss}|:[/] [white]{message}[/]"); // Change for deafult console
                    }
                    catch
                    {
                        // Needs to be beter, for example coloring
                        Console.WriteLine($"{userName}|{DateTime.Now:HH:mm:ss}|: {message}");
                    }
                    //Console.SetCursorPosition(0, windowHeight - 1);
                    //AnsiConsole.Markup($"[fuchsia]You[/][yellow]|{DateTime.Now:HH:mm:ss}|:[/] ");
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
                    case "/quit":
                        await connection.InvokeAsync("QuitRoom");
                        await ChooseRoom();
                        break;

                    case "/users":
                        // Needs improvemnts and move to own function
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

        public static void WriteLineMultithread(string strt)
        {
            int lastx = Console.CursorLeft;
            Console.WriteLine();
            int lasty = Console.BufferHeight - 1; // Buffer height
            // Source width handle mulitline?
            Console.SetCursorPosition(0, lasty);
            //Wyczyzczenie tej lini?
            //Console.WriteConsoleOutput
#pragma warning disable CA1416 // Validate platform compatibility
            Console.MoveBufferArea(0, lasty - 1, lastx, 1, 0, lasty, ' ', Console.ForegroundColor, Console.BackgroundColor);
            //Console.MoveBufferArea(0, 0, Console.BufferWidth)
#pragma warning restore CA1416 // Validate platform compatibility
            Console.SetCursorPosition(0, lasty - 1);
            Console.WriteLine(strt);
            Console.SetCursorPosition(lastx, lasty);
        }
    }
}