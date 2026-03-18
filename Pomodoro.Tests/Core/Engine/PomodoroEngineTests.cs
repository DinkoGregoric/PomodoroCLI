using AwesomeAssertions;
using NSubstitute;
using Pomodoro.Core.Common;
using Pomodoro.Core.Domain;
using Pomodoro.Core.Engine;
using Pomodoro.Core.Events;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.Tests.Core.Engine;

public class PomodoroEngineTests
{
    private static readonly DateTimeOffset Epoch = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static async Task<PomodoroStateMachine> CreateMachineAsync(
        FakeTimeProvider timeProvider,
        PomodoroSettings? settings = null)
    {
        settings ??= new PomodoroSettings();
        var provider = Substitute.For<ISettingsProvider>();
        provider.LoadSettingsAsync().Returns(Result<PomodoroSettings>.Success(settings));
        provider.SaveSettingsAsync(Arg.Any<PomodoroSettings>()).Returns(Task.FromResult(Result.Success()));
        return (await PomodoroStateMachine.CreateAsync(provider, timeProvider)).Value;
    }

    [Fact]
    public async Task State_ReturnsStateMachineState()
    {
        var machine = await CreateMachineAsync(new FakeTimeProvider(Epoch));
        var engine = new PomodoroEngine(machine, Substitute.For<ICommandDispatcher>());

        engine.State.Should().BeSameAs(machine.State);
    }

    [Fact]
    public async Task PhaseCompleted_WhenStateMachineFires_EngineForwardsEvent()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp); // WorkMinutes = 25
        var engine = new PomodoroEngine(machine, Substitute.For<ICommandDispatcher>());

        machine.Start();
        tp.Advance(TimeSpan.FromMinutes(25));

        PhaseCompletedEventArgs? received = null;
        engine.PhaseCompleted += (_, e) => received = e;

        await machine.Tick();

        received.Should().NotBeNull();
        received!.CompletedPhase.Should().Be(Phase.Work);
        received.NextPhase.Should().Be(Phase.ShortBreak);
    }

    [Fact]
    public async Task SessionExpiredDueToPauseTimeout_WhenStateMachineFires_EngineForwardsEvent()
    {
        var settings = new PomodoroSettings();
        settings.Timing.MaxPhasePauseMinutes = 3;
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp, settings);
        var engine = new PomodoroEngine(machine, Substitute.For<ICommandDispatcher>());

        machine.Start();
        machine.Pause();
        tp.Advance(TimeSpan.FromMinutes(4));

        SessionExpiredEventArgs? received = null;
        engine.SessionExpiredDueToPauseTimeout += (_, e) => received = e;

        await machine.Tick();

        received.Should().NotBeNull();
        received!.Phase.Should().Be(Phase.Work);
        received.MaxAllowedPauseDuration.Should().Be(3);
    }
}
