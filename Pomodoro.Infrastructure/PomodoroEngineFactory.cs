using Pomodoro.Core.Commands;
using Pomodoro.Core.Common;
using Pomodoro.Core.Engine;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.Infrastructure
{
    internal sealed class PomodoroEngineFactory(ISettingsProvider settingsProvider, TimeProvider timeProvider)
        : IPomodoroEngineFactory
    {
        public async Task<Result<PomodoroEngine>> CreateAsync()
        {
            var machineResult = await PomodoroStateMachine.CreateAsync(settingsProvider, timeProvider);
            if (machineResult.IsFailure)
                return Result<PomodoroEngine>.Failure(machineResult.Error);

            var sm = machineResult.Value;
            var dispatcher = new InMemoryCommandDispatcher();
            dispatcher.RegisterHandler(new StartCommandHandler(sm));
            dispatcher.RegisterHandler(new PauseCommandHandler(sm));
            dispatcher.RegisterHandler(new ResumeCommandHandler(sm));
            dispatcher.RegisterHandler(new AdvanceTimeCommandHandler(sm));
            dispatcher.RegisterHandler(new ResetPhaseCommandHandler(sm));
            dispatcher.RegisterHandler(new SkipPhaseCommandHandler(sm));

            return Result<PomodoroEngine>.Success(new PomodoroEngine(sm, dispatcher));

        }
    }
}
