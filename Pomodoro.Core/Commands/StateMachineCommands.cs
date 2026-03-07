using Pomodoro.Core.Common;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.Core.Commands
{
    public record StartCommand : ICommand<Result>;
    public record PauseCommand : ICommand<Result>;
    public record ResumeCommand : ICommand<Result>;
    public record AdvanceTimeCommand : ICommand<Result>;
    public record ResetPhaseCommand : ICommand<Result>;
    public record SkipPhaseCommand : ICommand<Result>;
}
