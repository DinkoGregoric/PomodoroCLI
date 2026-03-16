namespace Pomodoro.Core.Domain
{
    public sealed class PomodoroSettings
    {
        public TimingSettings Timing { get; set; } = new TimingSettings();
        public ProgressionSettings Progression { get; set; } = new ProgressionSettings();
        public NotificationSettings Notifications { get; set; } = new NotificationSettings();
        public DiagnosticsSettings Diagnostics { get; set; } = new DiagnosticsSettings();
    }

    public sealed class TimingSettings
    {
        public int WorkMinutes { get; set; } = 25;
        public int ShortBreakMinutes { get; set; } = 5;
        public int LongBreakMinutes { get; set; } = 15;
        public int LongBreakInterval { get; set; } = 4;
        public int MaxPhasePauseMinutes { get; set; } = 5;
    }

    public sealed class ProgressionSettings
    {
        public bool ProgressionEnabled { get; set; } = false;
        public int TargetWorkMinutes { get; set; } = 45;
        public int StepMinutes { get; set; } = 5;
        public int RequiredCompletionsToApplyStep { get; set; } = 10;
    }

    public sealed class NotificationSettings
    {
        public bool PlaySound { get; set; } = true;
    }

    public sealed class DiagnosticsSettings
    {
        public bool EnableLogging { get; set; } = false;
    }

}
