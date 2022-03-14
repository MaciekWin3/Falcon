using Falcon.Client.Models;
using Spectre.Console;

namespace Falcon.Client
{
    public class FalconOrchestrator
    {
        public void DisplayMenu()
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
            Run(choice);
        }

        private MenuOption ParseMenuChoice(string option)
        {
            return (MenuOption)int.Parse(option[0].ToString());
        }

        private void Run(MenuOption choice)
        {
            switch (choice)
            {
                case MenuOption.JoinChat:
                    JoinChat();
                    break;

                case MenuOption.Exit:
                    Exit();
                    break;

                case MenuOption.Help:
                    DisplayManual();
                    break;

                default:
                    Exit();
                    break;
            }
        }

        private void DisplayManual()
        {
            var rule = new Rule("[red]Hello[/]");
            AnsiConsole.Write(rule);
            AnsiConsole.MarkupLine("[magenta]Manual[/]");
            AnsiConsole.MarkupLine("[magenta]Press q to exit[/]");
            var x = Console.ReadKey();
            if (x.KeyChar == 'q')
            {
                Console.Clear();
                DisplayMenu();
            }
        }

        private static void JoinChat()
        {
            AnsiConsole.Markup("[magenta]Joined chat[/]");
        }

        private static void Exit()
        {
            AnsiConsole.Markup("[red]Exiting...[/]");
            Environment.Exit(1);
        }
    }
}