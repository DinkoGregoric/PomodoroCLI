using Pomodoro.Core.Domain;
using Pomodoro.Core.Interfaces;
using Pomodoro.Core.Common;
using Pomodoro.Core.Validation;

namespace Pomodoro.Core.Commands.Settings
{
    internal sealed class GetSettingsCommandHandler(ISettingsProvider settingsProvider) : ICommandHandler<GetSettingsCommand, Result<PomodoroSettings>>
    {
        public Task<Result<PomodoroSettings>> HandleAsync(GetSettingsCommand command, CancellationToken cancellationToken = default)
        {
            return settingsProvider.LoadSettingsAsync();
        }
    }

    internal sealed class SaveTimingSettingsCommandHandler(ISettingsProvider settingsProvider) : ICommandHandler<SaveTimingSettingsCommand, Result<PomodoroSettings>>
    {
        public async Task<Result<PomodoroSettings>> HandleAsync(SaveTimingSettingsCommand command, CancellationToken cancellationToken = default)
        {
            var validation = SettingsValidator.ValidateTiming(command);
            if (validation.IsFailure)
                return Result<PomodoroSettings>.Failure(validation.Error);

            var loadResult = await settingsProvider.LoadSettingsAsync();
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

            var saveResult = await settingsProvider.SaveSettingsAsync(settings);
            return saveResult.IsFailure
                ? Result<PomodoroSettings>.Failure(saveResult.Error)
                : Result<PomodoroSettings>.Success(settings);
        }
    }

    internal sealed class SaveProgressionSettingsCommandHandler(ISettingsProvider settingsProvider) : ICommandHandler<SaveProgressionSettingsCommand, Result<PomodoroSettings>>
    {
        public async Task<Result<PomodoroSettings>> HandleAsync(SaveProgressionSettingsCommand command, CancellationToken cancellationToken = default)
        {
            var validation = SettingsValidator.ValidateProgression(command);
            if (validation.IsFailure)
                return Result<PomodoroSettings>.Failure(validation.Error);

            var loadResult = await settingsProvider.LoadSettingsAsync();
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

            var saveResult = await settingsProvider.SaveSettingsAsync(settings);
            return saveResult.IsFailure
                ? Result<PomodoroSettings>.Failure(saveResult.Error)
                : Result<PomodoroSettings>.Success(settings);
        }
    }

    internal sealed class SaveNotificationSettingsCommandHandler(ISettingsProvider settingsProvider) : ICommandHandler<SaveNotificationSettingsCommand, Result<PomodoroSettings>>
    {
        public async Task<Result<PomodoroSettings>> HandleAsync(SaveNotificationSettingsCommand command, CancellationToken cancellationToken = default)
        {
            var loadResult = await settingsProvider.LoadSettingsAsync();
            if (loadResult.IsFailure)
            {
                return loadResult;
            }

            var settings = loadResult.Value;
            settings.Notifications = new NotificationSettings
            {
                PlaySound = command.PlaySound
            };

            var saveResult = await settingsProvider.SaveSettingsAsync(settings);
            return saveResult.IsFailure
                ? Result<PomodoroSettings>.Failure(saveResult.Error)
                : Result<PomodoroSettings>.Success(settings);
        }
    }

    internal sealed class SaveDiagnosticsSettingsCommandHandler(ISettingsProvider settingsProvider) : ICommandHandler<SaveDiagnosticsSettingsCommand, Result<PomodoroSettings>>
    {
        public async Task<Result<PomodoroSettings>> HandleAsync(SaveDiagnosticsSettingsCommand command, CancellationToken cancellationToken = default)
        {
            var loadResult = await settingsProvider.LoadSettingsAsync();
            if (loadResult.IsFailure)
            {
                return loadResult;
            }

            var settings = loadResult.Value;
            settings.Diagnostics = new DiagnosticsSettings
            {
                EnableLogging = command.EnableEventLogging
            };

            var saveResult = await settingsProvider.SaveSettingsAsync(settings);
            return saveResult.IsFailure
                ? Result<PomodoroSettings>.Failure(saveResult.Error)
                : Result<PomodoroSettings>.Success(settings);
        }
    }
}
