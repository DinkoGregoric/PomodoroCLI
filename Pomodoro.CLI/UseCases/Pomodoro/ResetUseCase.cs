using Pomodoro.Core.Commands;
using Pomodoro.Core.Common;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.CLI.UseCases.Pomodoro
{
    internal class ResetUseCase(ICommandDispatcher dispatcher)
    {
        internal async Task<Result> ExecuteAsync()
        {
            return await dispatcher.DispatchAsync(new ResetPhaseCommand());
        }
    }
}
