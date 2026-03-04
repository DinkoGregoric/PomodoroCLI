using Pomodoro.CLI;
using Pomodoro.CLI.UseCases.Settings;
using Pomodoro.CLI.Views;
using Pomodoro.Core.Commands;
using Pomodoro.Core.Commands.Settings;
using Pomodoro.Infrastructure;
using Spectre.Console;

var settingsProvider = new SettingsProvider();
var dispatcher = new InMemoryCommandDispatcher();
dispatcher.RegisterHandler(new GetSettingsCommandHandler(settingsProvider));
dispatcher.RegisterHandler(new SaveTimingSettingsCommandHandler(settingsProvider));
dispatcher.RegisterHandler(new SaveProgressionSettingsCommandHandler(settingsProvider));
dispatcher.RegisterHandler(new SaveNotificationSettingsCommandHandler(settingsProvider));
dispatcher.RegisterHandler(new SaveDiagnosticsSettingsCommandHandler(settingsProvider));

var getSettingsUseCase = new GetSettingsUseCase(dispatcher);
var saveSettingsUseCase = new SaveSettingsUseCase(dispatcher);

var menuView = new StartMenuView();
var settingsView = new SettingsView(getSettingsUseCase, saveSettingsUseCase);

while (true)
{
    var commandType = menuView.Display();

    if (commandType == CLICommandType.StartPomodoro)
    {
        // TODO: Start the Pomodoro session
        AnsiConsole.Write(new Markup($"Starting a new Pomodoro session... This feature is under development!\n", Styles.Default));
    }
    else if (commandType == CLICommandType.ConfigureSettings)
    {
        await settingsView.Display();
    }
    else
    {
        break;
    }
}

AnsiConsole.Write(new Markup($"Thank you for using [bold red]Pomodoro CLI[/]🍅! Goodbye! 👋\n", Styles.Default));