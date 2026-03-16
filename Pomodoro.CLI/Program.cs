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
await using var provider = services.BuildServiceProvider();

var engineFactory = provider.GetRequiredService<IPomodoroEngineFactory>();
var settingsFactory = provider.GetRequiredService<ISettingsEngineFactory>();
var timeProvider = provider.GetRequiredService<TimeProvider>();
var logger = provider.GetRequiredService<IAppLogger>();
await UpdateLoggerVerbosity();

logger.Info("App started");

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    logger.Info("App stopped via Ctrl+C");
    logger.Dispose();
    Environment.Exit(0);
};

var settingsView = CreateSettingsView();
var pomodoroView = await CreatePomodoroView();

while (true)
{
    var commandType = StartMenuView.Display();

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

logger.Info("App stopped");
AnsiConsole.Clear();
AnsiConsole.Write(new Markup("Thank you for using [bold red]Pomodoro CLI[/]🍅! Goodbye! 👋\n", Styles.Default));

SettingsView CreateSettingsView()
{
    var settingsEngineResult = settingsFactory.Create();
    if (settingsEngineResult.IsFailure)
    {
        logger.Error($"Failed to create settings engine: {settingsEngineResult.Error.Message}");
        AnsiConsole.Write(new Markup($"[red]Error loading settings engine:[/] {settingsEngineResult.Error.Message}\n"));
        Environment.Exit(1);
    }
    var settingsEngine = settingsEngineResult.Value;
    return new SettingsView(
        new GetSettingsUseCase(settingsEngine),
        new SaveSettingsUseCase(settingsEngine));
}

async Task<PomodoroView> CreatePomodoroView()
{
    var result = await engineFactory.CreateAsync();
    if (result.IsFailure)
    {
        logger.Error($"Failed to create Pomodoro engine: {result.Error.Message}");
        AnsiConsole.Write(new Markup($"[red]Error loading settings:[/] {result.Error.Message}\n"));
        Environment.Exit(1);
    }
    var engine = result.Value;
    await UpdateLoggerVerbosity();
    return new PomodoroView(
        timeProvider,
        engine,
        new StartUseCase(engine.Dispatcher),
        new PauseUseCase(engine.Dispatcher),
        new ResumeUseCase(engine.Dispatcher),
        new ResetUseCase(engine.Dispatcher),
        new SkipUseCase(engine.Dispatcher),
        logger);
}

async Task UpdateLoggerVerbosity()
{
    var engineResult = settingsFactory.Create();
    if (engineResult.IsFailure)
    {
        return;
    }

    var settings = await new GetSettingsUseCase(engineResult.Value).ExecuteAsync();
    if (settings.IsSuccess)
    {
        logger.EnableFileLogging(settings.Value.Diagnostics.EnableLogging);
    }
}
