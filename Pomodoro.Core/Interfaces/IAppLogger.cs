namespace Pomodoro.Core.Interfaces
{
    public interface IAppLogger : IDisposable
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception? ex = null);
        void EnableFileLogging(bool enabled);
    }
}
