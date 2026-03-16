using Pomodoro.Core.Commands.Settings;
using Pomodoro.Core.Common;
using Pomodoro.Core.Domain;
using Pomodoro.Core.Engine;

namespace Pomodoro.CLI.UseCases.Settings
{
    internal class SaveSettingsUseCase(SettingsEngine engine)
    {
        internal Task<Result<PomodoroSettings>> ExecuteForTimingSettingsAsync(
            int workMinutes,
            int shortBreakMinutes,
            int longBreakMinutes,
            int longBreakInterval,
            int maxPhasePauseMinutes)
        {
            return engine.Dispatcher.DispatchAsync(new SaveTimingSettingsCommand(
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
            return engine.Dispatcher.DispatchAsync(new SaveProgressionSettingsCommand(
                progressionEnabled,
                targetWorkMinutes,
                stepMinutes,
                requiredCompletionsToApplyStep));
        }

        internal Task<Result<PomodoroSettings>> ExecuteForNotificationSettingsAsync(bool playSound)
        {
            return engine.Dispatcher.DispatchAsync(new SaveNotificationSettingsCommand(playSound));
        }

        internal Task<Result<PomodoroSettings>> ExecuteForDiagnosticsSettingsAsync(bool enableEventLogging)
        {
            return engine.Dispatcher.DispatchAsync(new SaveDiagnosticsSettingsCommand(enableEventLogging));
        }
    }
}
