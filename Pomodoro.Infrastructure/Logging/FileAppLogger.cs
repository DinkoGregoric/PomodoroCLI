using Pomodoro.Core.Interfaces;

namespace Pomodoro.Infrastructure.Logging
{
    internal sealed class FileAppLogger : IAppLogger
    {
        private readonly StreamWriter? _writer;
        private readonly object _lock = new();
        private bool _enableEventLogging;
        private bool _disposed;

        public FileAppLogger()
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pomodoro");
                Directory.CreateDirectory(dir);
                _writer = OpenWriter(dir);
            }
            catch
            {
                _writer = null;
            }
        }

        internal FileAppLogger(string directory)
        {
            try
            {
                Directory.CreateDirectory(directory);
                _writer = OpenWriter(directory);
            }
            catch
            {
                _writer = null;
            }
        }

        private static StreamWriter OpenWriter(string directory) =>
            new(Path.Combine(directory, "pomodoro.log"), append: true) { AutoFlush = true };

        public void EnableFileLogging(bool enabled)
        {
            _enableEventLogging = enabled;
        }

        public void Info(string message) => Write("INFO", message);
        public void Warning(string message) => Write("WARN", message);
        public void Error(string message, Exception? ex = null) => Write("ERROR", message, ex);

        private void Write(string level, string message, Exception? ex = null)
        {
            if (!_enableEventLogging || _writer is null || _disposed)
            {
                return;
            }

            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                _writer.WriteLine($"{DateTimeOffset.UtcNow:O} [{level}] {message}");
                if (ex is not null)
                {
                    _writer.WriteLine(ex.ToString());
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                try
                {
                    _writer?.Flush();
                    _writer?.Dispose();
                }
                catch { }
            }
        }
    }
}
