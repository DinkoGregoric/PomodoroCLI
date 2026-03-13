using AwesomeAssertions;
using NSubstitute;
using Pomodoro.Core.Common;
using Pomodoro.Core.Domain;
using Pomodoro.Core.Engine;
using Pomodoro.Core.Events;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.Tests.Core.Engine;

public class PomodoroStateMachineTests
{
    private static readonly DateTimeOffset Epoch = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static async Task<PomodoroStateMachine> CreateMachineAsync(
        FakeTimeProvider timeProvider,
        PomodoroSettings? settings = null)
    {
        settings ??= new PomodoroSettings();
        var provider = Substitute.For<ISettingsProvider>();
        provider.LoadSettingsAsync().Returns(Result<PomodoroSettings>.Success(settings));
        return (await PomodoroStateMachine.CreateAsync(provider, timeProvider)).Value;
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_ProviderFailure_ReturnsFailure()
    {
        var provider = Substitute.For<ISettingsProvider>();
        var error = new Error("Settings.LoadFailed", "Failed to load.");
        provider.LoadSettingsAsync().Returns(Result<PomodoroSettings>.Failure(error));

        var result = await PomodoroStateMachine.CreateAsync(provider, new FakeTimeProvider(Epoch));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task CreateAsync_ProviderSuccess_ReturnsMachineInIdleState()
    {
        var provider = Substitute.For<ISettingsProvider>();
        provider.LoadSettingsAsync().Returns(Result<PomodoroSettings>.Success(new PomodoroSettings()));

        var result = await PomodoroStateMachine.CreateAsync(provider, new FakeTimeProvider(Epoch));

        result.IsSuccess.Should().BeTrue();
        result.Value.State.CurrentPhase.Should().Be(Phase.Idle);
    }

    // --- Start ---

    [Fact]
    public async Task Start_WhenIdle_InitializesWorkPhase()
    {
        var settings = new PomodoroSettings();
        settings.Timing.WorkMinutes = 30;
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp, settings);

        machine.Start();

        machine.State.CurrentPhase.Should().Be(Phase.Work);
        machine.State.PhaseStartTimeUtc.Should().Be(Epoch);
        machine.State.PhaseDuration.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public async Task Start_WhenAlreadyRunning_DoesNothing()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);
        machine.Start();
        var originalStartTime = machine.State.PhaseStartTimeUtc;

        tp.Advance(TimeSpan.FromSeconds(5));
        machine.Start();

        machine.State.PhaseStartTimeUtc.Should().Be(originalStartTime);
        machine.State.CurrentPhase.Should().Be(Phase.Work);
    }

    [Fact]
    public async Task Start_WhenPaused_DoesNothing()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);
        machine.Start();
        machine.Pause();
        var pausedAt = machine.State.PausedAtUtc;

        machine.Start();

        machine.State.PausedAtUtc.Should().Be(pausedAt);
        machine.State.IsRunning.Should().BeFalse();
    }

    // --- Pause ---

    [Fact]
    public async Task Pause_WhenRunning_SetsPausedAtUtc()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);
        machine.Start();
        tp.Advance(TimeSpan.FromMinutes(1));

        machine.Pause();

        machine.State.PausedAtUtc.Should().Be(Epoch + TimeSpan.FromMinutes(1));
        machine.State.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task Pause_WhenIdle_DoesNothing()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);

        machine.Pause();

        machine.State.PausedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task Pause_WhenAlreadyPaused_DoesNothing()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);
        machine.Start();
        machine.Pause();
        var firstPausedAt = machine.State.PausedAtUtc;

        tp.Advance(TimeSpan.FromSeconds(1));
        machine.Pause();

        machine.State.PausedAtUtc.Should().Be(firstPausedAt);
    }

    // --- Resume ---

    [Fact]
    public async Task Resume_WhenNotPaused_DoesNothing()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);
        machine.Start();

        machine.Resume();

        machine.State.IsRunning.Should().BeTrue();
        machine.State.PauseAccumulated.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task Resume_WithinMaxPause_ClearsPausedAtUtcAndAccumulatesDuration()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp); // MaxPhasePauseMinutes = 5
        machine.Start();
        machine.Pause();
        tp.Advance(TimeSpan.FromMinutes(2));

        machine.Resume();

        machine.State.PausedAtUtc.Should().BeNull();
        machine.State.PauseAccumulated.Should().Be(TimeSpan.FromMinutes(2));
        machine.State.IsRunning.Should().BeTrue();
    }

    [Fact]
    public async Task Resume_ExceedingMaxPause_RaisesEventAndResetsToIdle()
    {
        var settings = new PomodoroSettings();
        settings.Timing.MaxPhasePauseMinutes = 3;
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp, settings);
        machine.Start(); // Work phase
        machine.Pause();
        tp.Advance(TimeSpan.FromMinutes(4));

        SessionExpiredEventArgs? receivedArgs = null;
        machine.SessionExpiredDueToPauseTimeout += (_, e) => receivedArgs = e;

        machine.Resume();

        receivedArgs.Should().NotBeNull();
        receivedArgs.Phase.Should().Be(Phase.Work);
        receivedArgs.MaxAllowedPauseDuration.Should().Be(3);
        machine.State.CurrentPhase.Should().Be(Phase.Idle);
        machine.State.PhaseStartTimeUtc.Should().BeNull();
        machine.State.PausedAtUtc.Should().BeNull();
    }

    // --- Tick: pause timeout ---

    [Fact]
    public async Task Tick_WhenPausedWithinMaxTime_DoesNotExpireSession()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);
        machine.Start();
        machine.Pause();
        tp.Advance(TimeSpan.FromMinutes(4)); // < MaxPhasePauseMinutes (5)

        var expired = false;
        machine.SessionExpiredDueToPauseTimeout += (_, _) => expired = true;

        machine.Tick();

        expired.Should().BeFalse();
        machine.State.CurrentPhase.Should().Be(Phase.Work);
    }

    [Fact]
    public async Task Tick_WhenPausedExceedingMaxTime_RaisesEventAndResetsToIdle()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);
        machine.Start();
        machine.Pause();
        tp.Advance(TimeSpan.FromMinutes(6));

        var expired = false;
        machine.SessionExpiredDueToPauseTimeout += (_, _) => expired = true;

        machine.Tick();

        expired.Should().BeTrue();
        machine.State.CurrentPhase.Should().Be(Phase.Idle);
    }

    // --- Tick: phase completion ---

    [Fact]
    public async Task Tick_BeforePhaseCompletes_NoPhaseCompletedEventRaised()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp); // WorkMinutes = 25
        machine.Start();
        tp.Advance(TimeSpan.FromMinutes(24)); // 1 minute short

        var completed = false;
        machine.PhaseCompleted += (_, _) => completed = true;

        machine.Tick();

        completed.Should().BeFalse();
    }

    [Fact]
    public async Task Tick_WhenWorkPhaseCompletes_RaisesEventIncrementsCountAndTransitionsPhase()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);
        machine.Start();
        tp.Advance(TimeSpan.FromMinutes(25));

        PhaseCompletedEventArgs? receivedArgs = null;
        machine.PhaseCompleted += (_, e) => receivedArgs = e;

        machine.Tick();

        receivedArgs.Should().NotBeNull();
        receivedArgs!.CompletedPhase.Should().Be(Phase.Work);
        receivedArgs.NextPhase.Should().Be(Phase.ShortBreak);
        machine.State.CompletedWorkSessionsCount.Should().Be(1);
        machine.State.CurrentPhase.Should().Be(Phase.ShortBreak);
    }

    [Fact]
    public async Task Tick_WhenShortBreakCompletes_RaisesEventAndTransitionsToWork()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);
        machine.Start();
        tp.Advance(TimeSpan.FromMinutes(25));
        machine.Tick(); // Work completes → ShortBreak prepared

        machine.Start(); // start ShortBreak
        tp.Advance(TimeSpan.FromMinutes(5));

        PhaseCompletedEventArgs? receivedArgs = null;
        machine.PhaseCompleted += (_, e) => receivedArgs = e;

        machine.Tick();

        receivedArgs!.CompletedPhase.Should().Be(Phase.ShortBreak);
        machine.State.CompletedWorkSessionsCount.Should().Be(1); // unchanged
        machine.State.CurrentPhase.Should().Be(Phase.Work);
    }

    [Fact]
    public async Task Tick_WhenLongBreakCompletes_RaisesEventAndTransitionsToWork()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);
        machine.State.CompletedWorkSessionsCount = 3;
        machine.Start();
        tp.Advance(TimeSpan.FromMinutes(25));
        machine.Tick(); // Work completes (count=4) → LongBreak prepared

        machine.Start(); // start LongBreak
        tp.Advance(TimeSpan.FromMinutes(15));

        PhaseCompletedEventArgs? receivedArgs = null;
        machine.PhaseCompleted += (_, e) => receivedArgs = e;

        machine.Tick();

        receivedArgs!.CompletedPhase.Should().Be(Phase.LongBreak);
        machine.State.CurrentPhase.Should().Be(Phase.Work);
    }

    [Fact]
    public async Task Tick_PhaseCompletedEvent_CarriesPlaySoundFromSettings()
    {
        var settings = new PomodoroSettings();
        settings.Notifications.PlaySound = false;
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp, settings);
        machine.Start();
        tp.Advance(TimeSpan.FromMinutes(25));

        PhaseCompletedEventArgs? receivedArgs = null;
        machine.PhaseCompleted += (_, e) => receivedArgs = e;

        machine.Tick();

        receivedArgs!.PlaySound.Should().BeFalse();
    }

    // --- Tick: pause time excluded from elapsed ---

    [Fact]
    public async Task Tick_AfterResume_PauseTimeIsExcludedFromElapsed()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp); // WorkMinutes = 25
        machine.Start();

        tp.Advance(TimeSpan.FromMinutes(20));
        machine.Pause();
        tp.Advance(TimeSpan.FromMinutes(2)); // 2 min pause
        machine.Resume(); // PauseAccumulated = 2 min

        // elapsed = 22 total - 2 pause = 20 min < 25 min → should NOT complete
        tp.Advance(TimeSpan.FromMinutes(4)); // total = 26 min, elapsed = 24 min < 25

        var completed = false;
        machine.PhaseCompleted += (_, _) => completed = true;
        machine.Tick();
        completed.Should().BeFalse();

        // advance 1 more minute: total = 27 min, elapsed = 25 min → should complete
        tp.Advance(TimeSpan.FromMinutes(1));
        machine.Tick();
        completed.Should().BeTrue();
    }

    // --- Phase cycling ---

    [Fact]
    public async Task Tick_AtLongBreakInterval_NextPhaseIsLongBreak()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp); // LongBreakInterval = 4
        machine.State.CompletedWorkSessionsCount = 3; // next completion = 4th

        machine.Start();
        tp.Advance(TimeSpan.FromMinutes(25));
        machine.Tick();

        machine.State.CurrentPhase.Should().Be(Phase.LongBreak);
        machine.State.CompletedWorkSessionsCount.Should().Be(4);
    }

    [Fact]
    public async Task Tick_CustomLongBreakInterval_TriggersLongBreakAtCorrectCount()
    {
        var settings = new PomodoroSettings();
        settings.Timing.LongBreakInterval = 2;
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp, settings);
        machine.State.CompletedWorkSessionsCount = 1; // next = 2nd session

        machine.Start();
        tp.Advance(TimeSpan.FromMinutes(25));
        machine.Tick(); // count = 2, 2 % 2 == 0 → LongBreak

        machine.State.CurrentPhase.Should().Be(Phase.LongBreak);
    }

    // --- Reset ---

    [Fact]
    public async Task Reset_ClearsAllStateToIdle()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);
        machine.Start();
        machine.Pause();

        machine.Reset();

        machine.State.CurrentPhase.Should().Be(Phase.Idle);
        machine.State.PhaseStartTimeUtc.Should().BeNull();
        machine.State.PhaseDuration.Should().BeNull();
        machine.State.PausedAtUtc.Should().BeNull();
        machine.State.PauseAccumulated.Should().Be(TimeSpan.Zero);
    }

    // --- Skip ---

    [Fact]
    public async Task Skip_WhenIdle_DoesNothing()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);

        machine.Skip();

        machine.State.CurrentPhase.Should().Be(Phase.Idle);
    }

    [Fact]
    public async Task Skip_DuringWork_TransitionsToShortBreak()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);
        machine.Start(); // Work, CompletedWorkSessionsCount = 0

        machine.Skip(); // 0 % 4 != 0 → ShortBreak

        machine.State.CurrentPhase.Should().Be(Phase.ShortBreak);
    }

    [Fact]
    public async Task Skip_DuringShortBreak_TransitionsToWork()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);
        machine.Start();
        machine.Skip(); // Work → ShortBreak

        machine.Skip(); // ShortBreak → Work

        machine.State.CurrentPhase.Should().Be(Phase.Work);
    }

    [Fact]
    public async Task Skip_DuringLongBreak_TransitionsToWork()
    {
        var tp = new FakeTimeProvider(Epoch);
        var machine = await CreateMachineAsync(tp);
        machine.State.CompletedWorkSessionsCount = 4;
        machine.Start();
        machine.Skip(); // Work, count=4, 4 % 4 == 0 → LongBreak

        machine.Skip(); // LongBreak → Work

        machine.State.CurrentPhase.Should().Be(Phase.Work);
    }
}
