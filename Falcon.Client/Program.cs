using Falcon.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

var falconOrchestrator = new FalconOrchestrator();
var chat = new Chat();
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
//connection.On("ReceiveMessage", (string userName, string message) =>
//{
//    AnsiConsole.MarkupLine($"[blue]{userName}[/]: [green]{message}[/]");
//});

chat.Run();

//while (true)
//{
//    Console.SetCursorPosition(0, Console.WindowTop + Console.WindowHeight - 1);
//    Console.Write("Your message: ");
//    var message = Console.ReadLine();
//    connection.InvokeCoreAsync("SendMessageAsync", args: new[] { "Maciek", message });
//    message = string.Empty;
//}