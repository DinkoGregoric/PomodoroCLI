using Spectre.Console;

namespace Pomodoro.CLI.Views
{
    internal sealed class StartMenuView
    {
        internal CLICommandType Display()
        {
            AnsiConsole.Write(new Markup($"Welcome to [bold red]Pomodoro CLI[/] 🍅\n\n", Styles.Default));
            var command = AnsiConsole.Prompt(
                new SelectionPrompt<CLICommand>()
                    .Title($"[{Styles.Default.Foreground}]What would you like to do?[/]")
                    .UseConverter(cmd => $"[{Styles.Default.Foreground}]{cmd.Description}[/]")
                    .AddChoices(CLICommands.StartPomodoro, CLICommands.ConfigureSettings, CLICommands.Exit));

            return command.Type;
        }
    }
}
