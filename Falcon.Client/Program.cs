using Falcon.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Spectre.Console;

var falconOrchestrator = new FalconOrchestrator();
falconOrchestrator.DisplayMenu();

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7262/chathub")
    .ConfigureLogging(configureLogging =>
    {
        configureLogging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
        configureLogging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
    })
    .Build();

connection.StartAsync().Wait();
connection.On("ReceiveMessage", (string userName, string message) =>
{
    AnsiConsole.MarkupLine($"[blue]{userName}[/]: [green]{message}[/]");
});


while (true)
{
    var message = WriteOnBottomLine();
    connection.InvokeCoreAsync("SendMessageAsync", args: new[] { "Maciek", message });
    message = string.Empty;
}

static string WriteOnBottomLine()
{
    int x = Console.CursorLeft;
    int y = Console.CursorTop;
    Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1;
    var message = Console.ReadLine();
    Console.WriteLine(message);
    // Restore previous position
    Console.SetCursorPosition(x, y);
    return message;
}

Console.WriteLine("Done");

Console.ReadKey();
