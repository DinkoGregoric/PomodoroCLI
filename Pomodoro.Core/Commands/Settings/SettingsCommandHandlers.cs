using Pomodoro.Core.Domain;
using Pomodoro.Core.Interfaces;
using Pomodoro.Core.Common;

namespace Pomodoro.Core.Commands.Settings
{
    public sealed class GetSettingsCommandHandler : ICommandHandler<GetSettingsCommand, Result<PomodoroSettings>>
    {
        private readonly ISettingsProvider _settingsProvider;

        public GetSettingsCommandHandler(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public Task<Result<PomodoroSettings>> HandleAsync(GetSettingsCommand command, CancellationToken cancellationToken = default)
        {
            return _settingsProvider.LoadSettingsAsync();
        }
    }

    public sealed class SaveTimingSettingsCommandHandler : ICommandHandler<SaveTimingSettingsCommand, Result<PomodoroSettings>>
    {
        private readonly ISettingsProvider _settingsProvider;

        public SaveTimingSettingsCommandHandler(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task<Result<PomodoroSettings>> HandleAsync(SaveTimingSettingsCommand command, CancellationToken cancellationToken = default)
        {
            var loadResult = await _settingsProvider.LoadSettingsAsync();
            if (loadResult.IsFailure)
            {
                return loadResult;
            }

            var settings = loadResult.Value;
            settings.Timing = new TimingSettings
            {
                WorkMinutes = command.WorkMinutes,
                ShortBreakMinutes = command.ShortBreakMinutes,
                LongBreakMinutes = command.LongBreakMinutes,
                LongBreakInterval = command.LongBreakInterval,
                MaxPhasePauseMinutes = command.MaxPhasePauseMinutes
            };

            var saveResult = await _settingsProvider.SaveSettingsAsync(settings);
            return saveResult.IsFailure
                ? Result<PomodoroSettings>.Failure(saveResult.Error)
                : Result<PomodoroSettings>.Success(settings);
        }
    }

    public sealed class SaveProgressionSettingsCommandHandler : ICommandHandler<SaveProgressionSettingsCommand, Result<PomodoroSettings>>
    {
        private readonly ISettingsProvider _settingsProvider;

        public SaveProgressionSettingsCommandHandler(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task<Result<PomodoroSettings>> HandleAsync(SaveProgressionSettingsCommand command, CancellationToken cancellationToken = default)
        {
            var loadResult = await _settingsProvider.LoadSettingsAsync();
            if (loadResult.IsFailure)
            {
                return loadResult;
            }

            var settings = loadResult.Value;
            settings.Progression = new ProgressionSettings
            {
                ProgressionEnabled = command.ProgressionEnabled,
                TargetWorkMinutes = command.TargetWorkMinutes,
                StepMinutes = command.StepMinutes,
                RequiredCompletionsToApplyStep = command.RequiredCompletionsToApplyStep
            };

            var saveResult = await _settingsProvider.SaveSettingsAsync(settings);
            return saveResult.IsFailure
                ? Result<PomodoroSettings>.Failure(saveResult.Error)
                : Result<PomodoroSettings>.Success(settings);
        }
    }

    public sealed class SaveNotificationSettingsCommandHandler : ICommandHandler<SaveNotificationSettingsCommand, Result<PomodoroSettings>>
    {
        private readonly ISettingsProvider _settingsProvider;

        public SaveNotificationSettingsCommandHandler(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task<Result<PomodoroSettings>> HandleAsync(SaveNotificationSettingsCommand command, CancellationToken cancellationToken = default)
        {
            var loadResult = await _settingsProvider.LoadSettingsAsync();
            if (loadResult.IsFailure)
            {
                return loadResult;
            }

            var settings = loadResult.Value;
            settings.Notifications = new NotificationSettings
            {
                EnableNotifications = command.EnableNotifications,
                PlaySound = command.PlaySound,
                Sound = command.Sound,
                NotificationVolume = command.NotificationVolume
            };

            var saveResult = await _settingsProvider.SaveSettingsAsync(settings);
            return saveResult.IsFailure
                ? Result<PomodoroSettings>.Failure(saveResult.Error)
                : Result<PomodoroSettings>.Success(settings);
        }
    }

    public sealed class SaveDiagnosticsSettingsCommandHandler : ICommandHandler<SaveDiagnosticsSettingsCommand, Result<PomodoroSettings>>
    {
        private readonly ISettingsProvider _settingsProvider;

        public SaveDiagnosticsSettingsCommandHandler(ISettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task<Result<PomodoroSettings>> HandleAsync(SaveDiagnosticsSettingsCommand command, CancellationToken cancellationToken = default)
        {
            var loadResult = await _settingsProvider.LoadSettingsAsync();
            if (loadResult.IsFailure)
            {
                return loadResult;
            }

            var settings = loadResult.Value;
            settings.Diagnostics = new DiagnosticsSettings
            {
                EnableEventLogging = command.EnableEventLogging
            };

            var saveResult = await _settingsProvider.SaveSettingsAsync(settings);
            return saveResult.IsFailure
                ? Result<PomodoroSettings>.Failure(saveResult.Error)
                : Result<PomodoroSettings>.Success(settings);
        }
    }
}
