using Pomodoro.Core.Commands.Settings;
using Pomodoro.Core.Common;
using Pomodoro.Core.Domain;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.CLI.UseCases.Settings
{
    internal class GetSettingsUseCase(ICommandDispatcher dispatcher)
    {
        internal Task<Result<PomodoroSettings>> ExecuteAsync()
        {
            return dispatcher.DispatchAsync(new GetSettingsCommand());
        }
    }
}
