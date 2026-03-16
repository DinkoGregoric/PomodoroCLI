using Pomodoro.Core.Commands.Settings;
using Pomodoro.Core.Common;
using Pomodoro.Core.Domain;
using Pomodoro.Core.Engine;

namespace Pomodoro.CLI.UseCases.Settings
{
    internal class GetSettingsUseCase(SettingsEngine engine)
    {
        internal Task<Result<PomodoroSettings>> ExecuteAsync()
        {
            return engine.Dispatcher.DispatchAsync(new GetSettingsCommand());
        }
    }
}
