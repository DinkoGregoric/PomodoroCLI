namespace Pomodoro.Core.Domain
{
    public class PomodoroState
    {
        public Phase CurrentPhase { get; set; } = Phase.Idle;
        public int CompletedWorkSessionsCount { get; set; } = 0;
        public DateTimeOffset? PhaseStartTimeUtc { get; set; } = null;
        public TimeSpan? PhaseDuration { get; set; } = null;
        public DateTimeOffset? PausedAtUtc { get; set; } = null;
        public TimeSpan PauseAccumulated { get; set; } = TimeSpan.Zero;
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
