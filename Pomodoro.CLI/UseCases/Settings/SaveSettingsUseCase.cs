using Pomodoro.Core.Commands.Settings;
using Pomodoro.Core.Common;
using Pomodoro.Core.Domain;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.CLI.UseCases.Settings
{
    internal class SaveSettingsUseCase(ICommandDispatcher dispatcher)
    {
        internal Task<Result<PomodoroSettings>> ExecuteForTimingSettingsAsync(
            int workMinutes,
            int shortBreakMinutes,
            int longBreakMinutes,
            int longBreakInterval,
            int maxPhasePauseMinutes)
        {
            return dispatcher.DispatchAsync(new SaveTimingSettingsCommand(
                workMinutes,
                shortBreakMinutes,
                longBreakMinutes,
                longBreakInterval,
                maxPhasePauseMinutes));
        }

        internal Task<Result<PomodoroSettings>> ExecuteForProgressionSettingsAsync(
            bool progressionEnabled,
            int targetWorkMinutes,
            int stepMinutes,
            int requiredCompletionsToApplyStep)
        {
            return dispatcher.DispatchAsync(new SaveProgressionSettingsCommand(
                progressionEnabled,
                targetWorkMinutes,
                stepMinutes,
                requiredCompletionsToApplyStep));
        }

        internal Task<Result<PomodoroSettings>> ExecuteForNotificationSettingsAsync(bool playSound)
        {
            return dispatcher.DispatchAsync(new SaveNotificationSettingsCommand(playSound));
        }

        internal Task<Result<PomodoroSettings>> ExecuteForDiagnosticsSettingsAsync(bool enableEventLogging)
        {
            return dispatcher.DispatchAsync(new SaveDiagnosticsSettingsCommand(enableEventLogging));
        }
    }
}
