using Pomodoro.Core.Common;
using Pomodoro.Core.Engine;

namespace Pomodoro.Core.Interfaces
{
    public interface ISettingsEngineFactory
    {
        Result<SettingsEngine> Create();
    }
}
