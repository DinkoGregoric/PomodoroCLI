using Pomodoro.Core.Common;
using Pomodoro.Core.Domain;

namespace Pomodoro.Core.Interfaces
{
    public interface ISettingsProvider
    {
        Task<Result<PomodoroSettings>> LoadSettingsAsync();
        Task<Result> SaveSettingsAsync(PomodoroSettings settings);
    }
}
