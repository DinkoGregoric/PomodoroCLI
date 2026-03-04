using Pomodoro.Core.Domain;
using Pomodoro.Core.Commands.Settings;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.CLI.UseCases.Settings
{
    internal class SaveSettingsUseCase
    {
        private readonly ICommandDispatcher _dispatcher;

        public SaveSettingsUseCase(ICommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task ExecuteForTimingSettingsAsync(
            int workMinutes,
            int shortBreakMinutes,
            int longBreakMinutes,
            int longBreakInterval,
            int maxPhasePauseMinutes)
        {
            await _dispatcher.DispatchAsync(new SaveTimingSettingsCommand(
                workMinutes,
                shortBreakMinutes,
                longBreakMinutes,
                longBreakInterval,
                maxPhasePauseMinutes));
        }

        public async Task ExecuteForProgressionSettingsAsync(
            bool progressionEnabled,
            int targetWorkMinutes,
            int stepMinutes,
            int requiredCompletionsToApplyStep)
        {
            await _dispatcher.DispatchAsync(new SaveProgressionSettingsCommand(
                progressionEnabled,
                targetWorkMinutes,
                stepMinutes,
                requiredCompletionsToApplyStep));
        }

        public async Task ExecuteForNotificationSettingsAsync(
            bool enableNotifications,
            bool playSound,
            NotificationSound sound,
            int notificationVolume)
        {
            await _dispatcher.DispatchAsync(new SaveNotificationSettingsCommand(
                enableNotifications,
                playSound,
                sound,
                notificationVolume));
        }

        public async Task ExecuteForDiagnosticsSettingsAsync(bool enableEventLogging)
        {
            await _dispatcher.DispatchAsync(new SaveDiagnosticsSettingsCommand(enableEventLogging));
        }
    }
}
