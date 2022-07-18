using Falcon.Client.Enums;
using Spectre.Console;

namespace Falcon.Client.Services
{
    public class FalconOrchestratorService : IFalconOrchestratorService
    {
        private readonly IChatService chatService;

        public FalconOrchestratorService(IChatService chatService)
        {
            this.chatService = chatService;
        }

        public async Task DisplayMenu()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(
                new FigletText("Falcon")
                .LeftAligned()
                .Color(Color.Aqua));

            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[aqua]Menu[/]")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "1. Join chat",
                        "2. Help",
                        "3. Quit"
                    }));

            var choice = ParseMenuChoice(option);
            if (choice != MenuOption.Exit)
            {
                AnsiConsole.Clear();
            }
            await RunAsync(choice);
        }

        private static MenuOption ParseMenuChoice(string option)
        {
            return (MenuOption)int.Parse(option[0].ToString());
        }

        private async Task RunAsync(MenuOption choice)
        {
            switch (choice)
            {
                case MenuOption.JoinChat:
                    await JoinChat();
                    break;

                case MenuOption.Exit:
                    Exit();
                    break;

                case MenuOption.Help:
                    await DisplayManual();
                    break;

                default:
                    Exit();
                    break;
            }
        }

        private async Task DisplayManual()
        {
            var rule = new Rule("[red]Hello[/]");
            AnsiConsole.Write(rule);
            AnsiConsole.MarkupLine("[magenta]Manual[/]");
            AnsiConsole.MarkupLine("[magenta]Press q to exit[/]");
            var x = Console.ReadKey();
            if (x.KeyChar == 'q')
            {
                Console.Clear();
                await DisplayMenu();
            }
        }

        private async Task JoinChat()
        {
            await chatService.RunAsync();
        }

        private static void Exit()
        {
            AnsiConsole.Markup("[red]Exiting...[/]");
            Environment.Exit(1);
        }
    }
}