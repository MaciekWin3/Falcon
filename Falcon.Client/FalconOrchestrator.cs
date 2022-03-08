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

            // Ask for the user's favorite fruit
            var option = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[aqua]Menu[/]")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Nauraaaa)[/]")
                    .AddChoices(new[] {
                        "1. Join chat",
                        "2. Quit"
                    }));

            var choice = ParseMenuChoice(option);

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
                default:
                    Exit();
                    break;
            }
        }

        private void JoinChat()
        {
            AnsiConsole.Markup("[magenta]Joined chat[/]");
        }
        private void Exit()
        {
            AnsiConsole.Markup("[red]Exiting...[/]");
            Environment.Exit(1);
        }
    }
}
