using Pomodoro.Core.Domain;

namespace Pomodoro.Core.Interfaces
{
    public interface ISettingsProvider
    {
        Task<PomodoroSettings> LoadSettingsAsync();
        Task SaveSettingsAsync(PomodoroSettings settings);
    }
}
