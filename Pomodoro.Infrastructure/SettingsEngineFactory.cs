using Pomodoro.Core.Commands;
using Pomodoro.Core.Commands.Settings;
using Pomodoro.Core.Common;
using Pomodoro.Core.Engine;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.Infrastructure
{
    internal sealed class SettingsEngineFactory(ISettingsProvider settingsProvider) : ISettingsEngineFactory
    {
        public Result<SettingsEngine> Create()
        {
            var dispatcher = new InMemoryCommandDispatcher();
            dispatcher.RegisterHandler(new GetSettingsCommandHandler(settingsProvider));
            dispatcher.RegisterHandler(new SaveTimingSettingsCommandHandler(settingsProvider));
            dispatcher.RegisterHandler(new SaveProgressionSettingsCommandHandler(settingsProvider));
            dispatcher.RegisterHandler(new SaveNotificationSettingsCommandHandler(settingsProvider));
            dispatcher.RegisterHandler(new SaveDiagnosticsSettingsCommandHandler(settingsProvider));
            return Result<SettingsEngine>.Success(new SettingsEngine(dispatcher));
        }
    }
}
