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

// Settings dispatcher — created once, never needs recreating
var settingsDispatcher = new InMemoryCommandDispatcher();
settingsDispatcher.RegisterHandler(new GetSettingsCommandHandler(settingsProvider));
settingsDispatcher.RegisterHandler(new SaveTimingSettingsCommandHandler(settingsProvider));
settingsDispatcher.RegisterHandler(new SaveProgressionSettingsCommandHandler(settingsProvider));
settingsDispatcher.RegisterHandler(new SaveNotificationSettingsCommandHandler(settingsProvider));
settingsDispatcher.RegisterHandler(new SaveDiagnosticsSettingsCommandHandler(settingsProvider));

var getSettingsUseCase = new GetSettingsUseCase(settingsDispatcher);
var saveSettingsUseCase = new SaveSettingsUseCase(settingsDispatcher);

var menuView = new StartMenuView();
var settingsView = new SettingsView(getSettingsUseCase, saveSettingsUseCase);

var pomodoroView = await CreatePomodoroStack();

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
        pomodoroView = await CreatePomodoroStack();
        AnsiConsole.Clear();
    }
    else
    {
        break;
    }
}

AnsiConsole.Clear();
AnsiConsole.Write(new Markup($"Thank you for using [bold red]Pomodoro CLI[/]🍅! Goodbye! 👋\n", Styles.Default));


// Engine dispatcher + view — recreated after settings change
async Task<PomodoroView> CreatePomodoroStack()
{
    var engineResult = await PomodoroStateMachine.CreateAsync(settingsProvider, TimeProvider.System);
    if (engineResult.IsFailure)
    {
        AnsiConsole.Write(new Markup($"[red]Error loading settings:[/] {engineResult.Error.Message}\n"));
        Environment.Exit(1);
    }

    var engineDispatcher = new InMemoryCommandDispatcher();
    engineDispatcher.RegisterHandler(new StartCommandHandler(engineResult.Value));
    engineDispatcher.RegisterHandler(new PauseCommandHandler(engineResult.Value));
    engineDispatcher.RegisterHandler(new ResumeCommandHandler(engineResult.Value));
    engineDispatcher.RegisterHandler(new AdvanceTimeCommandHandler(engineResult.Value));
    engineDispatcher.RegisterHandler(new ResetPhaseCommandHandler(engineResult.Value));
    engineDispatcher.RegisterHandler(new SkipPhaseCommandHandler(engineResult.Value));

    var startUseCase = new StartUseCase(engineDispatcher);
    var pauseUseCase = new PauseUseCase(engineDispatcher);
    var resumeUseCase = new ResumeUseCase(engineDispatcher);
    var resetPhaseUseCase = new ResetUseCase(engineDispatcher);
    var skipPhaseUseCase = new SkipUseCase(engineDispatcher);

    return new PomodoroView(TimeProvider.System, engineResult.Value, engineDispatcher, startUseCase, pauseUseCase, resumeUseCase, resetPhaseUseCase, skipPhaseUseCase);
}