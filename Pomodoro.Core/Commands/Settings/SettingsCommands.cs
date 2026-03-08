using Pomodoro.Core.Domain;
using Pomodoro.Core.Interfaces;
using Pomodoro.Core.Common;

namespace Pomodoro.Core.Commands.Settings
{
    public sealed record GetSettingsCommand : ICommand<Result<PomodoroSettings>>;

    public sealed record SaveTimingSettingsCommand(
        int WorkMinutes,
        int ShortBreakMinutes,
        int LongBreakMinutes,
        int LongBreakInterval,
        int MaxPhasePauseMinutes) : ICommand<Result<PomodoroSettings>>;

    public sealed record SaveProgressionSettingsCommand(
        bool ProgressionEnabled,
        int TargetWorkMinutes,
        int StepMinutes,
        int RequiredCompletionsToApplyStep) : ICommand<Result<PomodoroSettings>>;

    public sealed record SaveNotificationSettingsCommand(
        bool PlaySound) : ICommand<Result<PomodoroSettings>>;

    public sealed record SaveDiagnosticsSettingsCommand(bool EnableEventLogging) : ICommand<Result<PomodoroSettings>>;
}
