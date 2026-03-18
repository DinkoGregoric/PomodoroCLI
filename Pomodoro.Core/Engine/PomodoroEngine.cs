using Pomodoro.Core.Domain;
using Pomodoro.Core.Events;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.Core.Engine
{
    public sealed class PomodoroEngine
    {
        private readonly PomodoroStateMachine _machine;

        public PomodoroState State => _machine.State;
        public PomodoroProgressionDetails ProgressionDetails
        {
            get
            {
                var s = _machine.Settings;
                return new PomodoroProgressionDetails(
                    s.Progression.ProgressionEnabled,
                    s.Timing.WorkMinutes,
                    s.Progression.TargetWorkMinutes,
                    s.Progression.SessionsCompletedTowardStep,
                    s.Progression.RequiredCompletionsToApplyStep
                );
            }
        }
        public ICommandDispatcher Dispatcher { get; }

        public event EventHandler<PhaseCompletedEventArgs>? PhaseCompleted;
        public event EventHandler<SessionExpiredEventArgs>? SessionExpiredDueToPauseTimeout;

        internal PomodoroEngine(PomodoroStateMachine machine, ICommandDispatcher dispatcher)
        {
            _machine = machine;
            Dispatcher = dispatcher;

            _machine.PhaseCompleted += (s, e) => PhaseCompleted?.Invoke(this, e);
            _machine.SessionExpiredDueToPauseTimeout += (s, e) => SessionExpiredDueToPauseTimeout?.Invoke(this, e);
        }
    }
}
