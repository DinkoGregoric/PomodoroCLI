using Pomodoro.Core.Common;
using Pomodoro.Core.Domain;
using Pomodoro.Core.Interfaces;
using System.Text.Json;

namespace Pomodoro.Infrastructure
{
    internal sealed class SettingsProvider : ISettingsProvider, IDisposable
    {
        private readonly string _settingsDir;
        private readonly string _settingsPath;
        private readonly SemaphoreSlim _fileLock = new(1, 1);

        private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

        public SettingsProvider()
        {
            _settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pomodoro");
            _settingsPath = Path.Combine(_settingsDir, "settings.json");
        }

        internal SettingsProvider(string configDirectory)
        {
            _settingsDir = configDirectory;
            _settingsPath = Path.Combine(_settingsDir, "settings.json");
        }

        public async Task<Result<PomodoroSettings>> LoadSettingsAsync()
        {
            await _fileLock.WaitAsync();
            try
            {
                if (!File.Exists(_settingsPath))
                {
                    var defaults = new PomodoroSettings();
                    var saveResult = await SaveCoreAsync(defaults);

                    return saveResult.IsFailure
                        ? Result<PomodoroSettings>.Failure(saveResult.Error)
                        : Result<PomodoroSettings>.Success(defaults);
                }

                PomodoroSettings? settings;
                using (var fileStream = File.OpenRead(_settingsPath))
                {
                    settings = await JsonSerializer.DeserializeAsync<PomodoroSettings>(fileStream, SerializerOptions);
                }

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
                var errorMessage = $"Access to settings file is denied. Please check permissions for: {_settingsPath}.\n{ex.Message}\n";
                return Result<PomodoroSettings>.Failure(new Error("Settings.AccessDenied", errorMessage));
            }
            catch (IOException ex)
            {
                var errorMessage = $"An I/O error occurred while accessing the settings file: {_settingsPath}.\n{ex.Message}\n";
                return Result<PomodoroSettings>.Failure(new Error("Settings.LoadFailed", errorMessage));
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public async Task<Result> SaveSettingsAsync(PomodoroSettings settings)
        {
            await _fileLock.WaitAsync();
            try
            {
                return await SaveCoreAsync(settings);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task<Result> SaveCoreAsync(PomodoroSettings settings)
        {
            try
            {
                Directory.CreateDirectory(_settingsDir);
                await using var fileStream = File.Create(_settingsPath);
                await JsonSerializer.SerializeAsync(fileStream, settings, SerializerOptions);
                return Result.Success();
            }
            catch (UnauthorizedAccessException ex)
            {
                var errorMessage = $"Access to settings file is denied. Please check permissions for: {_settingsPath}.\n{ex.Message}\n";
                return Result.Failure(new Error("Settings.AccessDenied", errorMessage));
            }
            catch (IOException ex)
            {
                var errorMessage = $"An I/O error occurred while saving the settings file: {_settingsPath}.\n{ex.Message}\n";
                return Result.Failure(new Error("Settings.SaveFailed", errorMessage));
            }
        }

        private async Task<Result<PomodoroSettings>> ResetToDefaultsAsync()
        {
            var defaults = new PomodoroSettings();
            var saveResult = await SaveCoreAsync(defaults);

            return saveResult.IsFailure
                ? Result<PomodoroSettings>.Failure(saveResult.Error)
                : Result<PomodoroSettings>.Success(defaults);
        }

        public void Dispose()
        {
            _fileLock.Dispose();
        }
    }
}
