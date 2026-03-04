using Pomodoro.Core.Commands.Settings;
using Pomodoro.Core.Domain;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.CLI.UseCases.Settings
{
    internal class GetSettingsUseCase
    {
        private readonly ICommandDispatcher _dispatcher;

        public GetSettingsUseCase(ICommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public Task<PomodoroSettings> ExecuteAsync()
        {
            return _dispatcher.DispatchAsync(new GetSettingsCommand());
        }
    }
}
