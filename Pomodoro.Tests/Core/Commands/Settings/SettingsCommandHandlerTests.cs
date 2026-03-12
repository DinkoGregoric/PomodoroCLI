using AwesomeAssertions;
using NSubstitute;
using Pomodoro.Core.Commands.Settings;
using Pomodoro.Core.Common;
using Pomodoro.Core.Domain;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.Tests.Core.Commands.Settings;

public class SettingsCommandHandlerTests
{
    private static readonly Error LoadError = new("Settings.LoadFailed", "Failed to load.");
    private static readonly Error SaveError = new("Settings.SaveFailed", "Failed to save.");

    private static ISettingsProvider ProviderThatLoads(PomodoroSettings? settings = null)
    {
        var provider = Substitute.For<ISettingsProvider>();
        provider.LoadSettingsAsync().Returns(Result<PomodoroSettings>.Success(settings ?? new PomodoroSettings()));
        provider.SaveSettingsAsync(Arg.Any<PomodoroSettings>()).Returns(Result.Success());
        return provider;
    }

    private static ISettingsProvider ProviderThatFailsOnLoad()
    {
        var provider = Substitute.For<ISettingsProvider>();
        provider.LoadSettingsAsync().Returns(Result<PomodoroSettings>.Failure(LoadError));
        return provider;
    }

    private static ISettingsProvider ProviderThatFailsOnSave()
    {
        var provider = Substitute.For<ISettingsProvider>();
        provider.LoadSettingsAsync().Returns(Result<PomodoroSettings>.Success(new PomodoroSettings()));
        provider.SaveSettingsAsync(Arg.Any<PomodoroSettings>()).Returns(Result.Failure(SaveError));
        return provider;
    }

    // --- GetSettingsCommandHandler ---

    [Fact]
    public async Task GetSettings_DelegatesToProvider_ReturnsResult()
    {
        var settings = new PomodoroSettings();
        settings.Timing.WorkMinutes = 30;
        var provider = ProviderThatLoads(settings);
        var handler = new GetSettingsCommandHandler(provider);

        var result = await handler.HandleAsync(new GetSettingsCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Timing.WorkMinutes.Should().Be(30);
    }

    [Fact]
    public async Task GetSettings_ProviderFailure_ReturnsFailure()
    {
        var provider = ProviderThatFailsOnLoad();
        var handler = new GetSettingsCommandHandler(provider);

        var result = await handler.HandleAsync(new GetSettingsCommand(), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoadError);
    }

    // --- SaveTimingSettingsCommandHandler ---

    [Fact]
    public async Task SaveTiming_InvalidCommand_ReturnsValidationFailure_NeverCallsProvider()
    {
        var provider = Substitute.For<ISettingsProvider>();
        var handler = new SaveTimingSettingsCommandHandler(provider);
        var cmd = new SaveTimingSettingsCommand(0, 5, 15, 4, 5); // WorkMinutes invalid

        var result = await handler.HandleAsync(cmd, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Timing.WorkMinutes.OutOfRange");
        await provider.DidNotReceive().LoadSettingsAsync();
    }

    [Fact]
    public async Task SaveTiming_ProviderLoadFailure_ReturnsFailure()
    {
        var provider = ProviderThatFailsOnLoad();
        var handler = new SaveTimingSettingsCommandHandler(provider);

        var result = await handler.HandleAsync(new SaveTimingSettingsCommand(25, 5, 15, 4, 5), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoadError);
        await provider.DidNotReceive().SaveSettingsAsync(Arg.Any<PomodoroSettings>());
    }

    [Fact]
    public async Task SaveTiming_ProviderSaveFailure_ReturnsFailure()
    {
        var provider = ProviderThatFailsOnSave();
        var handler = new SaveTimingSettingsCommandHandler(provider);

        var result = await handler.HandleAsync(new SaveTimingSettingsCommand(25, 5, 15, 4, 5), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SaveError);
    }

    [Fact]
    public async Task SaveTiming_ValidCommand_UpdatesAllTimingFields()
    {
        var provider = ProviderThatLoads();
        var handler = new SaveTimingSettingsCommandHandler(provider);

        var result = await handler.HandleAsync(new SaveTimingSettingsCommand(30, 8, 20, 3, 7), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Timing.WorkMinutes.Should().Be(30);
        result.Value.Timing.ShortBreakMinutes.Should().Be(8);
        result.Value.Timing.LongBreakMinutes.Should().Be(20);
        result.Value.Timing.LongBreakInterval.Should().Be(3);
        result.Value.Timing.MaxPhasePauseMinutes.Should().Be(7);
    }

    // --- SaveProgressionSettingsCommandHandler ---

    [Fact]
    public async Task SaveProgression_InvalidCommand_ReturnsValidationFailure_NeverCallsProvider()
    {
        var provider = Substitute.For<ISettingsProvider>();
        var handler = new SaveProgressionSettingsCommandHandler(provider);
        var cmd = new SaveProgressionSettingsCommand(true, 0, 5, 10); // target work minutes invalid

        var result = await handler.HandleAsync(cmd, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Progression.TargetWorkMinutes.OutOfRange");
        await provider.DidNotReceive().LoadSettingsAsync();
    }

    [Fact]
    public async Task SaveProgression_ProviderLoadFailure_ReturnsFailure()
    {
        var provider = ProviderThatFailsOnLoad();
        var handler = new SaveProgressionSettingsCommandHandler(provider);

        var result = await handler.HandleAsync(new SaveProgressionSettingsCommand(true, 45, 5, 10), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoadError);
        await provider.DidNotReceive().SaveSettingsAsync(Arg.Any<PomodoroSettings>());
    }

    [Fact]
    public async Task SaveProgression_ProviderSaveFailure_ReturnsFailure()
    {
        var provider = ProviderThatFailsOnSave();
        var handler = new SaveProgressionSettingsCommandHandler(provider);

        var result = await handler.HandleAsync(new SaveProgressionSettingsCommand(true, 45, 5, 10), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SaveError);
    }

    [Fact]
    public async Task SaveProgression_ValidCommand_UpdatesAllProgressionFields()
    {
        var provider = ProviderThatLoads();
        var handler = new SaveProgressionSettingsCommandHandler(provider);

        var result = await handler.HandleAsync(new SaveProgressionSettingsCommand(true, 60, 10, 5), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Progression.ProgressionEnabled.Should().BeTrue();
        result.Value.Progression.TargetWorkMinutes.Should().Be(60);
        result.Value.Progression.StepMinutes.Should().Be(10);
        result.Value.Progression.RequiredCompletionsToApplyStep.Should().Be(5);
    }

    // --- SaveNotificationSettingsCommandHandler ---

    [Fact]
    public async Task SaveNotification_ProviderLoadFailure_ReturnsFailure()
    {
        var provider = ProviderThatFailsOnLoad();
        var handler = new SaveNotificationSettingsCommandHandler(provider);

        var result = await handler.HandleAsync(new SaveNotificationSettingsCommand(PlaySound: false), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoadError);
    }

    [Fact]
    public async Task SaveNotification_ProviderSaveFailure_ReturnsFailure()
    {
        var provider = ProviderThatFailsOnSave();
        var handler = new SaveNotificationSettingsCommandHandler(provider);

        var result = await handler.HandleAsync(new SaveNotificationSettingsCommand(PlaySound: false), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SaveError);
    }

    [Fact]
    public async Task SaveNotification_ValidCommand_UpdatesPlaySound()
    {
        var provider = ProviderThatLoads();
        var handler = new SaveNotificationSettingsCommandHandler(provider);

        var result = await handler.HandleAsync(new SaveNotificationSettingsCommand(PlaySound: false), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Notifications.PlaySound.Should().BeFalse();
    }

    // --- SaveDiagnosticsSettingsCommandHandler ---

    [Fact]
    public async Task SaveDiagnostics_ProviderLoadFailure_ReturnsFailure()
    {
        var provider = ProviderThatFailsOnLoad();
        var handler = new SaveDiagnosticsSettingsCommandHandler(provider);

        var result = await handler.HandleAsync(new SaveDiagnosticsSettingsCommand(EnableEventLogging: false), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoadError);
    }

    [Fact]
    public async Task SaveDiagnostics_ProviderSaveFailure_ReturnsFailure()
    {
        var provider = ProviderThatFailsOnSave();
        var handler = new SaveDiagnosticsSettingsCommandHandler(provider);

        var result = await handler.HandleAsync(new SaveDiagnosticsSettingsCommand(EnableEventLogging: false), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SaveError);
    }

    [Fact]
    public async Task SaveDiagnostics_ValidCommand_UpdatesEnableEventLogging()
    {
        var provider = ProviderThatLoads();
        var handler = new SaveDiagnosticsSettingsCommandHandler(provider);

        var result = await handler.HandleAsync(new SaveDiagnosticsSettingsCommand(EnableEventLogging: false), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Diagnostics.EnableEventLogging.Should().BeFalse();
    }
}
