using Pomodoro.Core.Domain;
using Pomodoro.Core.Events;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.Core.Engine
{
    public sealed class PomodoroStateMachine
    {
        private readonly PomodoroSettings _settings;
        private readonly TimeProvider _timeProvider;

        public event EventHandler<SessionExpiredEventArgs>? SessionExpiredDueToPauseTimeout;

        public PomodoroState State { get; }

        private PomodoroStateMachine(TimeProvider timeProvider, PomodoroSettings settings)
        {
            State = new PomodoroState();
            _timeProvider = timeProvider;
            _settings = settings;
        }

        public static async Task<PomodoroStateMachine> CreateAsync(ISettingsProvider settingsProvider, TimeProvider timeProvider)
        {
            var settings = await settingsProvider.LoadSettingsAsync();
            if (settings.IsFailure)
            {
                throw new InvalidOperationException("Failed to load settings: " + settings.Error.Message);
            }

            return new PomodoroStateMachine(timeProvider, settings.Value);
        }

        internal void Start()
        {
            if (State.IsRunning || State.PausedAtUtc != null)
            {
                return;
            }

            if (State.CurrentPhase == Phase.Idle)
            {
                PrepareNextPhase();
            }
            State.PhaseStartTimeUtc = _timeProvider.GetUtcNow();
        }

        internal void Pause()
        {
            if (State.IsRunning && State.PausedAtUtc is null)
            {
                State.PausedAtUtc = _timeProvider.GetUtcNow();
            }
        }

        internal void Resume()
        {
            if (State.PausedAtUtc != null)
            {
                var pausedDuration = _timeProvider.GetUtcNow() - State.PausedAtUtc.Value;
                if (pausedDuration > TimeSpan.FromMinutes(_settings.Timing.MaxPhasePauseMinutes))
                {
                    var eventArgs = new SessionExpiredEventArgs(State.CurrentPhase, _settings.Timing.MaxPhasePauseMinutes);
                    OnSessionExpired(eventArgs);
                    Reset();
                    return;
                }
                State.PauseAccumulated += pausedDuration;
                State.PausedAtUtc = null;
            }
        }

        internal void Tick()
        {
            if (State.PausedAtUtc != null)
            {
                var pausedDuration = _timeProvider.GetUtcNow() - State.PausedAtUtc.Value;
                if (pausedDuration > TimeSpan.FromMinutes(_settings.Timing.MaxPhasePauseMinutes))
                {
                    var eventArgs = new SessionExpiredEventArgs(State.CurrentPhase, _settings.Timing.MaxPhasePauseMinutes);
                    OnSessionExpired(eventArgs);
                    Reset();
                    return;
                }
            }

            if (State.IsRunning && State.PhaseStartTimeUtc.HasValue && State.PhaseDuration.HasValue)
            {
                var elapsed = _timeProvider.GetUtcNow() - State.PhaseStartTimeUtc.Value - State.PauseAccumulated;
                if (elapsed >= State.PhaseDuration)
                {
                    if (State.CurrentPhase == Phase.Work)
                    {
                        State.CompletedWorkSessionsCount++;
                    }
                    PrepareNextPhase();
                }
            }
        }

        internal void Reset()
        {
            State.CurrentPhase = Phase.Idle;
            State.PhaseStartTimeUtc = null;
            State.PhaseDuration = null;
            State.PausedAtUtc = null;
            State.PauseAccumulated = TimeSpan.Zero;
        }

        internal void Skip()
        {
            if (State.CurrentPhase != Phase.Idle)
            {
                PrepareNextPhase();
            }
        }

        private void OnSessionExpired(SessionExpiredEventArgs e)
        {
            SessionExpiredDueToPauseTimeout?.Invoke(this, e);
        }

        private void PrepareNextPhase()
        {
            var nextPhase = State.CurrentPhase switch
            {
                Phase.Idle => Phase.Work,
                Phase.ShortBreak => Phase.Work,
                Phase.LongBreak => Phase.Work,
                Phase.Work => State.CompletedWorkSessionsCount != 0 && State.CompletedWorkSessionsCount % _settings.Timing.LongBreakInterval == 0
                    ? Phase.LongBreak
                    : Phase.ShortBreak,
                _ => throw new InvalidOperationException("Invalid phase.")
            };

            var phaseDuration = nextPhase switch
            {
                Phase.Work => TimeSpan.FromMinutes(_settings.Timing.WorkMinutes),
                Phase.ShortBreak => TimeSpan.FromMinutes(_settings.Timing.ShortBreakMinutes),
                Phase.LongBreak => TimeSpan.FromMinutes(_settings.Timing.LongBreakMinutes),
                _ => throw new InvalidOperationException("Invalid phase.")
            };

            State.CurrentPhase = nextPhase;
            State.PhaseDuration = phaseDuration;
            State.PhaseStartTimeUtc = null;
            State.PausedAtUtc = null;
            State.PauseAccumulated = TimeSpan.Zero;
        }
    }
}
