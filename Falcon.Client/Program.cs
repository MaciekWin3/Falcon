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
connection.InvokeCoreAsync("SendMessage", args: new[] { "Maciek", "Hello" });
connection.On("ReceiveMessage", (string userName, string message) =>
{
    AnsiConsole.Markup($"[blue]{userName}[/]: [green]{message}[/]");
    //Console.WriteLine(userName + ": " + message);
});

Console.WriteLine("Done");

Console.ReadKey();
