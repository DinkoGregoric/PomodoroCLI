using Pomodoro.CLI;
using Pomodoro.CLI.UseCases.Pomodoro;
using Pomodoro.CLI.UseCases.Settings;
using Pomodoro.CLI.Views;
using Pomodoro.Core.Commands;
using Pomodoro.Core.Commands.Settings;
using Pomodoro.Core.Engine;
using Pomodoro.Infrastructure;
using Spectre.Console;

var settingsProvider = new SettingsProvider();
var dispatcher = new InMemoryCommandDispatcher();
var engine = await PomodoroStateMachine.CreateAsync(settingsProvider, TimeProvider.System);

// Everything related to Pomodoro core will be moved to DI in the Core package and then registered here as a method
// Register state machine command handlers
dispatcher.RegisterHandler(new StartCommandHandler(engine));
dispatcher.RegisterHandler(new PauseCommandHandler(engine));
dispatcher.RegisterHandler(new ResumeCommandHandler(engine));
dispatcher.RegisterHandler(new AdvanceTimeCommandHandler(engine));
dispatcher.RegisterHandler(new ResetPhaseCommandHandler(engine));
dispatcher.RegisterHandler(new SkipPhaseCommandHandler(engine));

// Register settings command handlers
dispatcher.RegisterHandler(new GetSettingsCommandHandler(settingsProvider));
dispatcher.RegisterHandler(new SaveTimingSettingsCommandHandler(settingsProvider));
dispatcher.RegisterHandler(new SaveProgressionSettingsCommandHandler(settingsProvider));
dispatcher.RegisterHandler(new SaveNotificationSettingsCommandHandler(settingsProvider));
dispatcher.RegisterHandler(new SaveDiagnosticsSettingsCommandHandler(settingsProvider));

var getSettingsUseCase = new GetSettingsUseCase(dispatcher);
var saveSettingsUseCase = new SaveSettingsUseCase(dispatcher);

var startUseCase = new StartUseCase(dispatcher);
var pauseUseCase = new PauseUseCase(dispatcher);
var resumeUseCase = new ResumeUseCase(dispatcher);
var resetPhaseUseCase = new ResetUseCase(dispatcher);
var skipPhaseUseCase = new SkipUseCase(dispatcher);

var menuView = new StartMenuView();
var pomodoroView = new PomodoroView(engine.State, dispatcher, startUseCase, pauseUseCase, resumeUseCase, resetPhaseUseCase, skipPhaseUseCase);
var settingsView = new SettingsView(getSettingsUseCase, saveSettingsUseCase);

while (true)
{
    var commandType = menuView.Display();

    if (commandType == CLICommandType.StartPomodoro)
    {
        AnsiConsole.Clear();
        await pomodoroView.Display();
        AnsiConsole.Clear();
    }
    else if (commandType == CLICommandType.ConfigureSettings)
    {
        AnsiConsole.Clear();
        await settingsView.Display();
        engine = await PomodoroStateMachine.CreateAsync(settingsProvider, TimeProvider.System);
        AnsiConsole.Clear();
    }
    else
    {
        break;
    }
}

AnsiConsole.Clear();
AnsiConsole.Write(new Markup($"Thank you for using [bold red]Pomodoro CLI[/]🍅! Goodbye! 👋\n", Styles.Default));