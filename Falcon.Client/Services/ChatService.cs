using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Falcon.Client.Services
{
    public class ChatService : IChatService
    {
        private static readonly object bufferLock = new();
        private static int windowHeight = Console.BufferHeight;
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
            string token = await authService.Login();
            Console.Clear(); // Check it
            if (token.Length != 0)
            {
                var connection = new HubConnectionBuilder()
                   .WithUrl($"https://localhost:7262/chathub?access_token=" + token) // ???
                   .ConfigureLogging(configureLogging =>
                   {
                       configureLogging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                       configureLogging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                   })
                   .WithAutomaticReconnect() // Handle it
                   .Build();

                connection.StartAsync().Wait();
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
                                ($"[blue]{userName}[/][yellow] <{DateTime.Now.ToString("HH:mm:ss")}>[/]: [green]{message}[/]"); // Change for deafult console
                        }
                        catch
                        {
                            // Needs to be beter, for example coloring
                            Console.WriteLine($"{userName} <{DateTime.Now.ToString("HH:mm:ss")}>[/]: {message}");
                        }
                        Console.SetCursorPosition(0, windowHeight - 1);
                        AnsiConsole.Markup($"[blue]You[/][yellow]<{DateTime.Now.ToString("HH: mm:ss")}>[/]: ");
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
                                connection.InvokeCoreAsync("SendMessageAsync", args: new[] { "Maciek", message });
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