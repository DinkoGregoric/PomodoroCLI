namespace Pomodoro.Core.Domain
{
    public class PomodoroState
    {
        public Phase CurrentPhase { get; internal set; } = Phase.Idle;
        public int CompletedWorkSessionsCount { get; internal set; } = 0;
        public DateTimeOffset? PhaseStartTimeUtc { get; internal set; } = null;
        public TimeSpan? PhaseDuration { get; internal set; } = null;
        public DateTimeOffset? PausedAtUtc { get; internal set; } = null;
        public TimeSpan PauseAccumulated { get; internal set; } = TimeSpan.Zero;
        public bool IsRunning => CurrentPhase != Phase.Idle && PhaseStartTimeUtc != null && PausedAtUtc == null;
    }

    public enum Phase
    {
        Idle,
        Work,
        ShortBreak,
        LongBreak
    }
}
