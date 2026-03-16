using Pomodoro.Core.Interfaces;

namespace Pomodoro.Infrastructure.Logging
{
    internal sealed class FileAppLogger : IAppLogger
    {
        private readonly string _directory;
        private readonly object _lock = new();
        private StreamWriter? _writer;
        private bool _writerFailed;
        private bool _enableFileLogging;
        private bool _disposed;

        public FileAppLogger()
        {
            _directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pomodoro");
        }

        internal FileAppLogger(string directory)
        {
            _directory = directory;
        }

        private static StreamWriter OpenWriter(string directory) =>
            new(Path.Combine(directory, "pomodoro.log"), append: true) { AutoFlush = true };

        public void EnableFileLogging(bool enabled)
        {
            _enableFileLogging = enabled;
        }

        public void Info(string message) => Write("INFO", message);
        public void Warning(string message) => Write("WARN", message);
        public void Error(string message, Exception? ex = null) => Write("ERROR", message, ex);

        private void Write(string level, string message, Exception? ex = null)
        {
            if (!_enableFileLogging || _disposed)
            {
                return;
            }

            lock (_lock)
            {
                if (!_enableFileLogging || _disposed)
                {
                    return;
                }

                if (_writer is null && !_writerFailed)
                {
                    try
                    {
                        Directory.CreateDirectory(_directory);
                        _writer = OpenWriter(_directory);
                    }
                    catch
                    {
                        _writerFailed = true;
                        return;
                    }
                }

                if (_writer is null)
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
