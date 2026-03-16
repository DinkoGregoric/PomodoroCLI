using Microsoft.Extensions.DependencyInjection;
using Pomodoro.CLI;
using Pomodoro.CLI.UseCases.Pomodoro;
using Pomodoro.CLI.UseCases.Settings;
using Pomodoro.CLI.Views;
using Pomodoro.Core.Interfaces;
using Pomodoro.Infrastructure;
using Spectre.Console;

var services = new ServiceCollection();
services.AddPomodoro();
var provider = services.BuildServiceProvider();

var factory = provider.GetRequiredService<IPomodoroEngineFactory>();
var settingsDispatcher = provider.GetRequiredService<ICommandDispatcher>();
var timeProvider = provider.GetRequiredService<TimeProvider>();

var getSettingsUseCase = new GetSettingsUseCase(settingsDispatcher);
var saveSettingsUseCase = new SaveSettingsUseCase(settingsDispatcher);

var menuView = new StartMenuView();
var settingsView = new SettingsView(getSettingsUseCase, saveSettingsUseCase);
var pomodoroView = await CreatePomodoroView();

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
        pomodoroView = await CreatePomodoroView();
        AnsiConsole.Clear();
    }
    else
    {
        break;
    }
}

AnsiConsole.Clear();
AnsiConsole.Write(new Markup("Thank you for using [bold red]Pomodoro CLI[/]🍅! Goodbye! 👋\n", Styles.Default));

async Task<PomodoroView> CreatePomodoroView()
{
    var result = await factory.CreateAsync();
    if (result.IsFailure)
    {
        AnsiConsole.Write(new Markup($"[red]Error loading settings:[/] {result.Error.Message}\n"));
        Environment.Exit(1);
    }
    var engine = result.Value;
    return new PomodoroView(
        timeProvider, engine,
        new StartUseCase(engine.Dispatcher),
        new PauseUseCase(engine.Dispatcher),
        new ResumeUseCase(engine.Dispatcher),
        new ResetUseCase(engine.Dispatcher),
        new SkipUseCase(engine.Dispatcher));
}
