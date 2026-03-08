using Pomodoro.CLI.UseCases.Settings;
using Pomodoro.Core.Domain;
using Spectre.Console;

namespace Pomodoro.CLI.Views
{
    internal class SettingsView
    {
        private readonly GetSettingsUseCase _getSettings;
        private readonly SaveSettingsUseCase _saveSettings;

        public SettingsView(GetSettingsUseCase getSettingsUseCase, SaveSettingsUseCase saveSettingsUseCase)
        {
            _getSettings = getSettingsUseCase;
            _saveSettings = saveSettingsUseCase;
        }

        internal async Task Display()
        {
            while (true)
            {
                var settingsResult = await _getSettings.ExecuteAsync();

                if (settingsResult.IsFailure)
                {
                    AnsiConsole.Write(new Markup($"[red]Failed to load settings: {settingsResult.Error.Message}[/]\n"));
                    break;
                }

                var settings = settingsResult.Value;

                AnsiConsole.Clear();
                AnsiConsole.Write(new Markup($"[bold]Pomodoro Settings Configuration[/]\n\n", Styles.Default));

                var settingCategoryCommand = AnsiConsole.Prompt(
                    new SelectionPrompt<CLICommand>()
                        .Title($"[{Styles.Default.Foreground}]What would you like to configure?[/]")
                        .UseConverter(cmd => $"[{Styles.Default.Foreground}]{cmd.Description}[/]")
                        .AddChoices(CLICommands.ConfigureTimingSettings, CLICommands.ConfigureProgressionSettings, CLICommands.ConfigureNotificationSettings, CLICommands.ConfigureDiagnosticsSettings, CLICommands.Exit));

                if (settingCategoryCommand.Type == CLICommandType.Exit)
                {
                    break;
                }

                switch (settingCategoryCommand.Type)
                {
                    case CLICommandType.ConfigureTimingSettings:
                        await ConfigureTimingSettings(settings);
                        break;
                    case CLICommandType.ConfigureProgressionSettings:
                        await ConfigureProgressionSettings(settings);
                        break;
                    case CLICommandType.ConfigureNotificationSettings:
                        await ConfigureNotificationSettings(settings);
                        break;
                    case CLICommandType.ConfigureDiagnosticsSettings:
                        await ConfigureDiagnosticsSettings(settings);
                        break;
                }
            }
        }

        private async Task ConfigureTimingSettings(PomodoroSettings settings)
        {
            var workMinutes = AnsiConsole.Prompt(
                new TextPrompt<int>($"[{Styles.Default.Foreground}]Work duration (minutes):[/]")
                    .DefaultValue(settings.Timing.WorkMinutes)
                    .ValidationErrorMessage($"[red]Please enter a valid number[/]")
                    .Validate(m => m > 0 && m <= 120 ? ValidationResult.Success() : ValidationResult.Error("[red]Must be between 1 and 120[/]")));

            var shortBreakMinutes = AnsiConsole.Prompt(
                new TextPrompt<int>($"[{Styles.Default.Foreground}]Short break duration (minutes):[/]")
                    .DefaultValue(settings.Timing.ShortBreakMinutes)
                    .ValidationErrorMessage($"[red]Please enter a valid number[/]")
                    .Validate(m => m > 0 && m <= 30 ? ValidationResult.Success() : ValidationResult.Error("[red]Must be between 1 and 30[/]")));

            var longBreakMinutes = AnsiConsole.Prompt(
                new TextPrompt<int>($"[{Styles.Default.Foreground}]Long break duration (minutes):[/]")
                    .DefaultValue(settings.Timing.LongBreakMinutes)
                    .ValidationErrorMessage($"[red]Please enter a valid number[/]")
                    .Validate(m => m > 0 && m <= 60 ? ValidationResult.Success() : ValidationResult.Error("[red]Must be between 1 and 60[/]")));

            var longBreakInterval = AnsiConsole.Prompt(
                new TextPrompt<int>($"[{Styles.Default.Foreground}]Long break interval (after how many work sessions):[/]")
                    .DefaultValue(settings.Timing.LongBreakInterval)
                    .ValidationErrorMessage($"[red]Please enter a valid number[/]")
                    .Validate(m => m > 0 && m <= 10 ? ValidationResult.Success() : ValidationResult.Error("[red]Must be between 1 and 10[/]")));

            var maxPhasePauseMinutes = AnsiConsole.Prompt(
                new TextPrompt<int>($"[{Styles.Default.Foreground}]Maximum pause duration before reset (minutes):[/]")
                    .DefaultValue(settings.Timing.MaxPhasePauseMinutes)
                    .ValidationErrorMessage($"[red]Please enter a valid number[/]")
                    .Validate(m => m > 0 && m <= 30 ? ValidationResult.Success() : ValidationResult.Error("[red]Must be between 1 and 30[/]")));

            var result = await _saveSettings.ExecuteForTimingSettingsAsync(
                workMinutes,
                shortBreakMinutes,
                longBreakMinutes,
                longBreakInterval,
                maxPhasePauseMinutes);

            if (result.IsFailure)
            {
                AnsiConsole.Write(new Markup($"[red]Failed to save timing settings: {result.Error.Message}[/]\n"));
            }
        }

        private async Task ConfigureProgressionSettings(PomodoroSettings settings)
        {
            var progressionEnabled = AnsiConsole.Confirm(
                $"[{Styles.Default.Foreground}]Enable progressive work duration?[/]",
                settings.Progression.ProgressionEnabled);

            var targetWorkMinutes = settings.Progression.TargetWorkMinutes;
            var stepMinutes = settings.Progression.StepMinutes;
            var requiredCompletionsToApplyStep = settings.Progression.RequiredCompletionsToApplyStep;

            if (progressionEnabled)
            {
                targetWorkMinutes = AnsiConsole.Prompt(
                    new TextPrompt<int>($"[{Styles.Default.Foreground}]Target work duration (minutes):[/]")
                        .DefaultValue(settings.Progression.TargetWorkMinutes)
                        .ValidationErrorMessage($"[red]Please enter a valid number[/]")
                        .Validate(m => m > 0 && m <= 120 ? ValidationResult.Success() : ValidationResult.Error("[red]Must be between 1 and 120[/]")));

                stepMinutes = AnsiConsole.Prompt(
                    new TextPrompt<int>($"[{Styles.Default.Foreground}]Step increase (minutes):[/]")
                        .DefaultValue(settings.Progression.StepMinutes)
                        .ValidationErrorMessage($"[red]Please enter a valid number[/]")
                        .Validate(m => m > 0 && m <= 15 ? ValidationResult.Success() : ValidationResult.Error("[red]Must be between 1 and 15[/]")));

                requiredCompletionsToApplyStep = AnsiConsole.Prompt(
                    new TextPrompt<int>($"[{Styles.Default.Foreground}]Required completions before increasing duration:[/]")
                        .DefaultValue(settings.Progression.RequiredCompletionsToApplyStep)
                        .ValidationErrorMessage($"[red]Please enter a valid number[/]")
                        .Validate(m => m > 0 && m <= 50 ? ValidationResult.Success() : ValidationResult.Error("[red]Must be between 1 and 50[/]")));
            }

            var result = await _saveSettings.ExecuteForProgressionSettingsAsync(
                progressionEnabled,
                targetWorkMinutes,
                stepMinutes,
                requiredCompletionsToApplyStep);

            if (result.IsFailure)
            {
                AnsiConsole.Write(new Markup($"[red]Failed to save progression settings: {result.Error.Message}[/]\n"));
            }
        }

        private async Task ConfigureNotificationSettings(PomodoroSettings settings)
        {
            var playSound = AnsiConsole.Confirm(
                    $"[{Styles.Default.Foreground}]Play sound after end of phase?[/]",
                    settings.Notifications.PlaySound);

            var result = await _saveSettings.ExecuteForNotificationSettingsAsync(playSound);

            if (result.IsFailure)
            {
                AnsiConsole.Write(new Markup($"[red]Failed to save notification settings: {result.Error.Message}[/]\n"));
            }
        }

        private async Task ConfigureDiagnosticsSettings(PomodoroSettings settings)
        {
            var enableEventLogging = AnsiConsole.Confirm(
                $"[{Styles.Default.Foreground}]Enable event logging?[/]",
                settings.Diagnostics.EnableEventLogging);

            var result = await _saveSettings.ExecuteForDiagnosticsSettingsAsync(enableEventLogging);

            if (result.IsFailure)
            {
                AnsiConsole.Write(new Markup($"[red]Failed to save diagnostics settings: {result.Error.Message}[/]\n"));
            }
        }
    }
}
