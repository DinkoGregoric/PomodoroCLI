using Microsoft.Extensions.DependencyInjection;
using Pomodoro.Core.Commands;
using Pomodoro.Core.Commands.Settings;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPomodoro(this IServiceCollection services)
        {
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<ISettingsProvider, SettingsProvider>();

            services.AddSingleton<ICommandDispatcher>(sp =>
            {
                var p = sp.GetRequiredService<ISettingsProvider>();
                var d = new InMemoryCommandDispatcher();
                d.RegisterHandler(new GetSettingsCommandHandler(p));
                d.RegisterHandler(new SaveTimingSettingsCommandHandler(p));
                d.RegisterHandler(new SaveProgressionSettingsCommandHandler(p));
                d.RegisterHandler(new SaveNotificationSettingsCommandHandler(p));
                d.RegisterHandler(new SaveDiagnosticsSettingsCommandHandler(p));
                return d;
            });

            services.AddSingleton<IPomodoroEngineFactory, PomodoroEngineFactory>();

            return services;
        }
    }
}
