namespace Pomodoro.Core.Domain
{
    public sealed class PomodoroSettings
    {
        public TimingSettings Timing { get; set; } = new TimingSettings();
        public ProgressionSettings Progression { get; set; } = new ProgressionSettings();
        public NotificationSettings Notifications { get; set; } = new NotificationSettings();
        public DiagnosticsSettings Diagnostics { get; set; } = new DiagnosticsSettings();
        public Information Info { get; set; } = new Information();
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

        // ToDo: Possible regressions, max allowed fails etc...
    }

    public sealed class NotificationSettings
    {
        public bool EnableNotifications { get; set; } = true;
        public bool PlaySound { get; set; } = true;
        public NotificationSound Sound { get; set; } = NotificationSound.Sound1; 
        public int NotificationVolume { get; set; } = 100;
    }

    public sealed class Information
    {
        public int Version { get; set; } = 1;

        public DateTimeOffset LastModifiedUtc { get; set; }
    }

    public sealed class DiagnosticsSettings
    {
        public bool EnableEventLogging { get; set; } = true;
    }

    public enum NotificationSound
    {
        Sound1,
        Sound2,
        Sound3
    }
}
