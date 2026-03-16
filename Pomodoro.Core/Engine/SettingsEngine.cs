using Pomodoro.Core.Interfaces;

namespace Pomodoro.Core.Engine
{
    public sealed class SettingsEngine
    {
        public ICommandDispatcher Dispatcher { get; }

        internal SettingsEngine(ICommandDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }
    }
}
