using Pomodoro.Core.Domain;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.Core.Commands.Settings
{
    public sealed record GetSettingsCommand : ICommand<PomodoroSettings>;

    public sealed record SaveTimingSettingsCommand(
        int WorkMinutes,
        int ShortBreakMinutes,
        int LongBreakMinutes,
        int LongBreakInterval,
        int MaxPhasePauseMinutes) : ICommand<PomodoroSettings>;

    public sealed record SaveProgressionSettingsCommand(
        bool ProgressionEnabled,
        int TargetWorkMinutes,
        int StepMinutes,
        int RequiredCompletionsToApplyStep) : ICommand<PomodoroSettings>;

    public sealed record SaveNotificationSettingsCommand(
        bool EnableNotifications,
        bool PlaySound,
        NotificationSound Sound,
        int NotificationVolume) : ICommand<PomodoroSettings>;

    public sealed record SaveDiagnosticsSettingsCommand(bool EnableEventLogging) : ICommand<PomodoroSettings>;
}
