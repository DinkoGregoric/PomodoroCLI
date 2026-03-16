using Pomodoro.Core.Common;
using Pomodoro.Core.Engine;

namespace Pomodoro.Core.Interfaces
{
    public interface IPomodoroEngineFactory
    {
        Task<Result<PomodoroEngine>> CreateAsync();
    }
}
