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

        public async Task<PomodoroSettings> LoadSettingsAsync()
        {
            await FileLock.WaitAsync();
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    FileLock.Release();
                    return await CreateDefaultSettings();
                }

                using var fileStream = File.OpenRead(SettingsPath);
                var settings = await JsonSerializer.DeserializeAsync<PomodoroSettings>(fileStream, SerializerOptions);

                if (settings is null)
                {
                    // delete the corrupted file and create a new one with default settings
                    File.Delete(SettingsPath);
                    FileLock.Release();
                    return await CreateDefaultSettings();
                }

                return settings;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"Access denied when loading settings from {SettingsPath}", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Failed to load settings from {SettingsPath}", ex);
            }
            catch (JsonException)
            {
                // Corrupted JSON - delete and recreate
                File.Delete(SettingsPath);
                FileLock.Release();
                return await CreateDefaultSettings();
            }
            finally
            {
                if (FileLock.CurrentCount == 0) // Ensure we only release if we actually acquired the lock
                {
                    FileLock.Release();
                }
            }
        }

        public async Task SaveSettingsAsync(PomodoroSettings settings)
        {
            await FileLock.WaitAsync();
            try
            {
                Directory.CreateDirectory(SettingsDir);
                using var fileStream = File.Create(SettingsPath);
                await JsonSerializer.SerializeAsync(fileStream, settings, SerializerOptions);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"Access denied when saving settings to {SettingsPath}", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Failed to save settings to {SettingsPath}", ex);
            }
            finally
            {
                if (FileLock.CurrentCount == 0) // Ensure we only release if we actually acquired the lock
                {
                    FileLock.Release();
                }
            }
        }

        private async Task<PomodoroSettings> CreateDefaultSettings()
        {
            var defaultSettings = new PomodoroSettings();
            await SaveSettingsAsync(defaultSettings);
            return defaultSettings;
        }
    }
}
