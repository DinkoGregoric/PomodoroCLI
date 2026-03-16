using Pomodoro.Core.Common;
using Pomodoro.Core.Engine;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.Core.Commands
{
    internal sealed class StartCommandHandler(PomodoroStateMachine stateMachine) : ICommandHandler<StartCommand, Result>
    {
        public Task<Result> HandleAsync(StartCommand command, CancellationToken cancellationToken = default)
        {
            stateMachine.Start();
            return Task.FromResult(Result.Success());
        }
    }

    internal sealed class PauseCommandHandler(PomodoroStateMachine stateMachine) : ICommandHandler<PauseCommand, Result>
    {
        public Task<Result> HandleAsync(PauseCommand command, CancellationToken cancellationToken = default)
        {
            stateMachine.Pause();
            return Task.FromResult(Result.Success());
        }
    }

    internal sealed class ResumeCommandHandler(PomodoroStateMachine stateMachine) : ICommandHandler<ResumeCommand, Result>
    {
        public Task<Result> HandleAsync(ResumeCommand command, CancellationToken cancellationToken = default)
        {
            stateMachine.Resume();
            return Task.FromResult(Result.Success());
        }
    }

    internal sealed class AdvanceTimeCommandHandler(PomodoroStateMachine stateMachine) : ICommandHandler<AdvanceTimeCommand, Result>
    {
        public Task<Result> HandleAsync(AdvanceTimeCommand command, CancellationToken cancellationToken = default)
        {
            stateMachine.Tick();
            return Task.FromResult(Result.Success());
        }
    }

    internal sealed class ResetPhaseCommandHandler(PomodoroStateMachine stateMachine) : ICommandHandler<ResetPhaseCommand, Result>
    {
        public Task<Result> HandleAsync(ResetPhaseCommand command, CancellationToken cancellationToken = default)
        {
            stateMachine.Reset();
            return Task.FromResult(Result.Success());
        }
    }

    internal sealed class SkipPhaseCommandHandler(PomodoroStateMachine stateMachine) : ICommandHandler<SkipPhaseCommand, Result>
    {
        public Task<Result> HandleAsync(SkipPhaseCommand command, CancellationToken cancellationToken = default)
        {
            stateMachine.Skip();
            return Task.FromResult(Result.Success());
        }
    }
}
