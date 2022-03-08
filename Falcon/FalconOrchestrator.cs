using Falcon.Client.Models;
using Spectre.Console;

namespace Falcon
{
    public class FalconOrchestrator
    {
        public void DisplayMenu()
        {
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

            if (option == "2. Quit")
            {

            }
            else
            {
                //var

            }
        }


        private void Run(MenuOption choice)
        {
            switch (choice)
            {
                case MenuOption.JoinChat:

            }
        }

        private MenuOption ParseMenuChoice(string option)
        {
            return (MenuOption)int.Parse(option[0].ToString());
        }

        public void JoinChat()
        {
            AnsiConsole.Markup("[pink]Joined chat[/]");
        }
        public void Exit()
        {
            AnsiConsole.Markup("[red]Exiting...[/]");
            Environment.Exit(1);
        }
    }
}
