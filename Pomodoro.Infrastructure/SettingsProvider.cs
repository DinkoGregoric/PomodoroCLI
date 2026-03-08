using Pomodoro.Core.Common;
using Pomodoro.Core.Domain;
using Pomodoro.Core.Interfaces;
using System.Text.Json;

namespace Pomodoro.Infrastructure
{
    public class SettingsProvider : ISettingsProvider
    {
        private static readonly string SettingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pomodoro");

        private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

        private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

        private static readonly SemaphoreSlim FileLock = new(1, 1);

        public async Task<Result<PomodoroSettings>> LoadSettingsAsync()
        {
            await FileLock.WaitAsync();
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    var defaults = new PomodoroSettings();
                    var saveResult = await SaveCoreAsync(defaults);

                    return saveResult.IsFailure
                        ? Result<PomodoroSettings>.Failure(saveResult.Error)
                        : Result<PomodoroSettings>.Success(defaults);
                }

                await using var fileStream = File.OpenRead(SettingsPath);
                var settings = await JsonSerializer.DeserializeAsync<PomodoroSettings>(fileStream, SerializerOptions);

                if (settings is not null)
                {
                    return Result<PomodoroSettings>.Success(settings);
                }

                return await ResetToDefaultsAsync();
            }
            catch (JsonException)
            {
                return await ResetToDefaultsAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                var errorMessage = $"Access to settings file is denied. Please check permissions for: {SettingsPath}.\n{ex.Message}\n";
                return Result<PomodoroSettings>.Failure(new Error("Settings.AccessDenied", errorMessage));
            }
            catch (IOException ex)
            {
                var errorMessage = $"An I/O error occurred while accessing the settings file: {SettingsPath}.\n{ex.Message}\n";
                return Result<PomodoroSettings>.Failure(new Error("Settings.LoadFailed", errorMessage));
            }
            finally
            {
                FileLock.Release();
            }
        }

        public async Task<Result> SaveSettingsAsync(PomodoroSettings settings)
        {
            await FileLock.WaitAsync();
            try
            {
                return await SaveCoreAsync(settings);
            }
            finally
            {
                FileLock.Release();
            }
        }

        private static async Task<Result> SaveCoreAsync(PomodoroSettings settings)
        {
            try
            {
                Directory.CreateDirectory(SettingsDir);
                await using var fileStream = File.Create(SettingsPath);
                await JsonSerializer.SerializeAsync(fileStream, settings, SerializerOptions);
                return Result.Success();
            }
            catch (UnauthorizedAccessException ex)
            {
                var errorMessage = $"Access to settings file is denied. Please check permissions for: {SettingsPath}.\n{ex.Message}\n";
                return Result.Failure(new Error("Settings.AccessDenied", errorMessage));
            }
            catch (IOException ex)
            {
                var errorMessage = $"An I/O error occurred while saving the settings file: {SettingsPath}.\n{ex.Message}\n";
                return Result.Failure(new Error("Settings.SaveFailed", errorMessage));
            }
        }

        private static async Task<Result<PomodoroSettings>> ResetToDefaultsAsync()
        {
            var defaults = new PomodoroSettings();
            var saveResult = await SaveCoreAsync(defaults);

            return saveResult.IsFailure
                ? Result<PomodoroSettings>.Failure(saveResult.Error)
                : Result<PomodoroSettings>.Success(defaults);
        }
    }
}
