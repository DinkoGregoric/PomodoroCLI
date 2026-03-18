using AwesomeAssertions;
using NSubstitute;
using Pomodoro.Core.Commands;
using Pomodoro.Core.Common;
using Pomodoro.Core.Domain;
using Pomodoro.Core.Interfaces;
using Pomodoro.Infrastructure;
using Pomodoro.Tests.Core.Engine;

namespace Pomodoro.Tests.Infrastructure;

public class PomodoroEngineFactoryTests
{
    private static readonly DateTimeOffset Epoch = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static PomodoroEngineFactory CreateFactory(ISettingsProvider provider)
    {
        provider.SaveSettingsAsync(Arg.Any<PomodoroSettings>()).Returns(Task.FromResult(Result.Success()));
        return new(provider, new FakeTimeProvider(Epoch));
    }

    [Fact]
    public async Task CreateAsync_WhenSettingsLoadFails_ReturnsFailure()
    {
        var error = new Error("Settings.LoadFailed", "Failed to load.");
        var provider = Substitute.For<ISettingsProvider>();
        provider.LoadSettingsAsync().Returns(Result<PomodoroSettings>.Failure(error));

        var result = await CreateFactory(provider).CreateAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task CreateAsync_WhenSettingsLoadSucceeds_ReturnsEngineInIdleState()
    {
        var provider = Substitute.For<ISettingsProvider>();
        provider.LoadSettingsAsync().Returns(Result<PomodoroSettings>.Success(new PomodoroSettings()));

        var result = await CreateFactory(provider).CreateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.CurrentPhase.Should().Be(Phase.Idle);
    }

    [Fact]
    public async Task CreateAsync_Dispatcher_CanDispatchStartCommand()
    {
        var provider = Substitute.For<ISettingsProvider>();
        provider.LoadSettingsAsync().Returns(Result<PomodoroSettings>.Success(new PomodoroSettings()));
        var engine = (await CreateFactory(provider).CreateAsync()).Value;

        await engine.Dispatcher.DispatchAsync(new StartCommand(), TestContext.Current.CancellationToken);

        engine.State.CurrentPhase.Should().Be(Phase.Work);
    }

    [Fact]
    public async Task CreateAsync_PhaseCompleted_IsForwardedThroughEngine()
    {
        var provider = Substitute.For<ISettingsProvider>();
        provider.LoadSettingsAsync().Returns(Result<PomodoroSettings>.Success(new PomodoroSettings()));
        var tp = new FakeTimeProvider(Epoch);
        var engine = (await new PomodoroEngineFactory(provider, tp).CreateAsync()).Value;

        await engine.Dispatcher.DispatchAsync(new StartCommand(), TestContext.Current.CancellationToken);
        tp.Advance(TimeSpan.FromMinutes(25));

        var fired = false;
        engine.PhaseCompleted += (_, _) => fired = true;

        await engine.Dispatcher.DispatchAsync(new AdvanceTimeCommand(), TestContext.Current.CancellationToken);

        fired.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_SessionExpired_IsForwardedThroughEngine()
    {
        var settings = new PomodoroSettings();
        settings.Timing.MaxPhasePauseMinutes = 3;
        var provider = Substitute.For<ISettingsProvider>();
        provider.LoadSettingsAsync().Returns(Result<PomodoroSettings>.Success(settings));
        var tp = new FakeTimeProvider(Epoch);
        var engine = (await new PomodoroEngineFactory(provider, tp).CreateAsync()).Value;

        await engine.Dispatcher.DispatchAsync(new StartCommand(), TestContext.Current.CancellationToken);
        await engine.Dispatcher.DispatchAsync(new PauseCommand(), TestContext.Current.CancellationToken);
        tp.Advance(TimeSpan.FromMinutes(4));

        var fired = false;
        engine.SessionExpiredDueToPauseTimeout += (_, _) => fired = true;

        await engine.Dispatcher.DispatchAsync(new AdvanceTimeCommand(), TestContext.Current.CancellationToken);

        fired.Should().BeTrue();
    }
}
