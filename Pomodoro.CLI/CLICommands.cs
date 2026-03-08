namespace Pomodoro.CLI
{
    internal static class CLICommands
    {
        internal static CLICommand StartPomodoro = new(CLICommandType.StartPomodoro, "Start a new Pomodoro session");
        internal static CLICommand ConfigureSettings = new(CLICommandType.ConfigureSettings, "Configure your Pomodoro settings");
        internal static CLICommand Exit = new(CLICommandType.Exit, "Exit");
        internal static CLICommand ConfigureTimingSettings = new(CLICommandType.ConfigureTimingSettings, "Timing");
        internal static CLICommand ConfigureProgressionSettings = new(CLICommandType.ConfigureProgressionSettings, "Progression");
        internal static CLICommand ConfigureNotificationSettings = new(CLICommandType.ConfigureNotificationSettings, "Notifications");
        internal static CLICommand ConfigureDiagnosticsSettings = new(CLICommandType.ConfigureDiagnosticsSettings, "Diagnostics");
    }

    internal class CLICommand(CLICommandType type, string description)
    {
        public CLICommandType Type { get; } = type;
        public string Description { get; } = description;
    }

    internal enum CLICommandType
    {
        StartPomodoro,
        ConfigureSettings,
        Exit,
        ConfigureTimingSettings,
        ConfigureProgressionSettings,
        ConfigureNotificationSettings,
        ConfigureDiagnosticsSettings
    }
}
