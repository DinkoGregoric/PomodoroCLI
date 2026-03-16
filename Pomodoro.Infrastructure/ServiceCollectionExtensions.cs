using Microsoft.Extensions.DependencyInjection;
using Pomodoro.Core.Interfaces;
using Pomodoro.Infrastructure.Logging;

namespace Pomodoro.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPomodoro(this IServiceCollection services)
        {
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<IAppLogger, FileAppLogger>();
            services.AddSingleton<ISettingsProvider, SettingsProvider>();
            services.AddSingleton<ISettingsEngineFactory, SettingsEngineFactory>();
            services.AddSingleton<IPomodoroEngineFactory, PomodoroEngineFactory>();

            return services;
        }
    }
}
