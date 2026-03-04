using Pomodoro.Core.Domain;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.Core.Commands.Settings
{
    public sealed class GetSettingsCommandHandler : ICommandHandler<GetSettingsCommand, PomodoroSettings>
    {
        private readonly ISettingsProvider _settingsProvider;

        public GetSettingsCommandHandler(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public Task<PomodoroSettings> HandleAsync(GetSettingsCommand command, CancellationToken cancellationToken = default)
        {
            return _settingsProvider.LoadSettingsAsync();
        }
    }

    public sealed class SaveTimingSettingsCommandHandler : ICommandHandler<SaveTimingSettingsCommand, PomodoroSettings>
    {
        private readonly ISettingsProvider _settingsProvider;

        public SaveTimingSettingsCommandHandler(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task<PomodoroSettings> HandleAsync(SaveTimingSettingsCommand command, CancellationToken cancellationToken = default)
        {
            var settings = await _settingsProvider.LoadSettingsAsync();
            settings.Timing = new TimingSettings
            {
                WorkMinutes = command.WorkMinutes,
                ShortBreakMinutes = command.ShortBreakMinutes,
                LongBreakMinutes = command.LongBreakMinutes,
                LongBreakInterval = command.LongBreakInterval,
                MaxPhasePauseMinutes = command.MaxPhasePauseMinutes
            };

            await _settingsProvider.SaveSettingsAsync(settings);
            return settings;
        }
    }

    public sealed class SaveProgressionSettingsCommandHandler : ICommandHandler<SaveProgressionSettingsCommand, PomodoroSettings>
    {
        private readonly ISettingsProvider _settingsProvider;

        public SaveProgressionSettingsCommandHandler(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task<PomodoroSettings> HandleAsync(SaveProgressionSettingsCommand command, CancellationToken cancellationToken = default)
        {
            var settings = await _settingsProvider.LoadSettingsAsync();
            settings.Progression = new ProgressionSettings
            {
                ProgressionEnabled = command.ProgressionEnabled,
                TargetWorkMinutes = command.TargetWorkMinutes,
                StepMinutes = command.StepMinutes,
                RequiredCompletionsToApplyStep = command.RequiredCompletionsToApplyStep
            };

            await _settingsProvider.SaveSettingsAsync(settings);
            return settings;
        }
    }

    public sealed class SaveNotificationSettingsCommandHandler : ICommandHandler<SaveNotificationSettingsCommand, PomodoroSettings>
    {
        private readonly ISettingsProvider _settingsProvider;

        public SaveNotificationSettingsCommandHandler(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task<PomodoroSettings> HandleAsync(SaveNotificationSettingsCommand command, CancellationToken cancellationToken = default)
        {
            var settings = await _settingsProvider.LoadSettingsAsync();
            settings.Notifications = new NotificationSettings
            {
                EnableNotifications = command.EnableNotifications,
                PlaySound = command.PlaySound,
                Sound = command.Sound,
                NotificationVolume = command.NotificationVolume
            };

            await _settingsProvider.SaveSettingsAsync(settings);
            return settings;
        }
    }

    public sealed class SaveDiagnosticsSettingsCommandHandler : ICommandHandler<SaveDiagnosticsSettingsCommand, PomodoroSettings>
    {
        private readonly ISettingsProvider _settingsProvider;

        public SaveDiagnosticsSettingsCommandHandler(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task<PomodoroSettings> HandleAsync(SaveDiagnosticsSettingsCommand command, CancellationToken cancellationToken = default)
        {
            var settings = await _settingsProvider.LoadSettingsAsync();
            settings.Diagnostics = new DiagnosticsSettings
            {
                EnableEventLogging = command.EnableEventLogging
            };

            await _settingsProvider.SaveSettingsAsync(settings);
            return settings;
        }
    }
}
