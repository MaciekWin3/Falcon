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

        public ChatService(IAuthService authService)
        {
            this.authService = authService;
        }

        public async Task RunAsync()
        {
            string token = await authService.Login();
            //bool isAuthorized = true;
            if (token.Length != 0)
            {
                var connection = new HubConnectionBuilder()
                   .WithUrl($"https://localhost:7262/chathub?access_token=" + token) // ???
                   .ConfigureLogging(configureLogging =>
                   {
                       configureLogging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
                       configureLogging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
                   })
                   .Build();

                connection.StartAsync().Wait();
                connection.On("ReceiveMessage", (string userName, string message) =>
                {
                    lock (bufferLock)
                    {
                        Console.CursorVisible = false;
                        Console.SetCursorPosition(0, windowHeight - 1);
                        AnsiConsole.MarkupLine($"[blue]{userName}[/]: [green]{message}[/]"); // Change for deafult console
                        Console.SetCursorPosition(0, windowHeight - 1);
                        Console.Write("Message: ");
                        Console.CursorVisible = true;
                    }
                });

                string? message;
                do
                {
                    lock (bufferLock)
                    {
                        Console.SetCursorPosition(0, windowHeight - 1);
                        Console.Write("Message: ");
                        Console.CursorVisible = true;
                    }

                    message = Console.ReadLine();
                    if (message is not null || message![0] != '/')
                    {
                        lock (bufferLock)
                        {
                            connection.InvokeCoreAsync("SendMessageAsync", args: new[] { "Maciek", message });
                            Console.CursorVisible = false;
                        }
                    }
                    else
                    {
                        ExecuteCommand();
                    }
                } while (!string.Equals(message, "quit()", StringComparison.OrdinalIgnoreCase));
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