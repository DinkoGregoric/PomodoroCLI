using Pomodoro.Core.Domain;

namespace Pomodoro.Core.Events
{
    public class SessionExpiredEventArgs(
        Phase Phase,
        int MaxAllowedPauseDuration) : EventArgs
    {
        public Phase Phase { get; } = Phase;
        public int MaxAllowedPauseDuration { get; } = MaxAllowedPauseDuration;
    }
}
