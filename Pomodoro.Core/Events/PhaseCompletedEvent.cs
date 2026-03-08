using Pomodoro.Core.Domain;

namespace Pomodoro.Core.Events
{
    public class PhaseCompletedEventArgs(Phase CompletedPhase, Phase NextPhase, bool PlaySound) : EventArgs
    {
        public Phase CompletedPhase { get; } = CompletedPhase;
        public Phase NextPhase { get; } = NextPhase;
        public bool PlaySound { get; } = PlaySound;
    }
}
