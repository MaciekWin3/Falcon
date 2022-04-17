﻿using Microsoft.AspNetCore.SignalR.Client;
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

        private readonly List<string> Commands = new()
        {
            "/quit",
            "/exit",
            "/active",
            "/chart"
        };

        public ChatService(IAuthService authService)
        {
            this.authService = authService;
        }

        public async Task RunAsync()
        {
            await ConnectionInitializer();
            await ChooseRoom();
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
                    if (message is not null || message![0] != '/')
                    {
                        lock (bufferLock)
                        {
                            connection.InvokeCoreAsync("SendGroupMessageAsync", args: new[] { message });
                            Console.CursorVisible = false;
                        }
                    }
                    if (message[0] == '/' && Commands.Contains(message)) // Is this necesary?
                    {
                        await ExecuteCommand(message);
                    }
                }
            } while (!string.Equals(message, "/exit", StringComparison.OrdinalIgnoreCase));
        }

        protected async Task ConnectionInitializer()
        {
            string token = await authService.Login();
            if (token.Length != 0)
            {
                connection = new HubConnectionBuilder()
                   //.WithUrl($"http://192.168.1.25:5262/chathub?access_token=" + token)
                   .WithUrl($"https://localhost:7262/chathub?access_token=" + token)
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
                            ($"[lime]{userName}[/][yellow]|{DateTime.Now:HH:mm:ss}|:[/] [white]{message}[/]"); // Change for deafult console
                    }
                    catch
                    {
                        // Needs to be beter, for example coloring
                        Console.WriteLine($"{userName}|{DateTime.Now:HH:mm:ss}|: {message}");
                    }
                    Console.SetCursorPosition(0, windowHeight - 1);
                    AnsiConsole.Markup($"[fuchsia]You[/][yellow]|{DateTime.Now:HH:mm:ss}|:[/] ");
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

            await connection.InvokeCoreAsync("JoinRoom", args: new[] { room });
        }

        private async Task ExecuteCommand(string command)
        {
            switch (command)
            {
                case "/quit":
                    await connection.InvokeAsync("QuitRoom");
                    await ChooseRoom();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}