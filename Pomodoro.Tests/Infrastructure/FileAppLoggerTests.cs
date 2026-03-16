using AwesomeAssertions;
using Pomodoro.Infrastructure.Logging;

namespace Pomodoro.Tests.Infrastructure;

public class FileAppLoggerTests
{
    [Fact]
    public void Info_WhenVerbosityDisabled_WritesNothing()
    {
        using var dir = new TempDirectory();
        using var logger = new FileAppLogger(dir.Path);

        logger.Info("should not appear");
        logger.Dispose();

        var lines = ReadLog(dir.Path);
        lines.Should().BeEmpty();
    }

    [Fact]
    public void Info_WhenVerbosityEnabled_WritesEntry()
    {
        using var dir = new TempDirectory();
        using var logger = new FileAppLogger(dir.Path);
        logger.EnableFileLogging(true);

        logger.Info("phase completed");
        logger.Dispose();

        var lines = ReadLog(dir.Path);
        lines.Should().HaveCount(1);
        lines[0].Should().Contain("[INFO]").And.Contain("phase completed");
    }

    [Fact]
    public void Warning_WhenEnabled_WritesEntry()
    {
        using var dir = new TempDirectory();
        using var logger = new FileAppLogger(dir.Path);
        logger.EnableFileLogging(true);

        logger.Warning("something unexpected");
        logger.Dispose();

        var lines = ReadLog(dir.Path);
        lines.Should().HaveCount(1);
        lines[0].Should().Contain("[WARN]").And.Contain("something unexpected");
    }

    [Fact]
    public void Error_WhenEnabled_WritesEntry()
    {
        using var dir = new TempDirectory();
        using var logger = new FileAppLogger(dir.Path);
        logger.EnableFileLogging(true);

        logger.Error("something broke");
        logger.Dispose();

        var lines = ReadLog(dir.Path);
        lines.Should().HaveCount(1);
        lines[0].Should().Contain("[ERROR]").And.Contain("something broke");
    }

    [Fact]
    public void Warning_WhenDisabled_WritesNothing()
    {
        using var dir = new TempDirectory();
        using var logger = new FileAppLogger(dir.Path);

        logger.Warning("should not appear");
        logger.Dispose();

        ReadLog(dir.Path).Should().BeEmpty();
    }

    [Fact]
    public void Error_WhenDisabled_WritesNothing()
    {
        using var dir = new TempDirectory();
        using var logger = new FileAppLogger(dir.Path);

        logger.Error("should not appear");
        logger.Dispose();

        ReadLog(dir.Path).Should().BeEmpty();
    }

    [Fact]
    public void Error_WithException_WritesExceptionDetail()
    {
        using var dir = new TempDirectory();
        using var logger = new FileAppLogger(dir.Path);
        logger.EnableFileLogging(true);
        var ex = new InvalidOperationException("bad state");

        logger.Error("something broke", ex);
        logger.Dispose();

        var content = File.ReadAllText(Path.Combine(dir.Path, "pomodoro.log"));
        content.Should().Contain("System.InvalidOperationException").And.Contain("bad state");
    }

    [Fact]
    public void LogEntry_HasExpectedFormat()
    {
        using var dir = new TempDirectory();
        using var logger = new FileAppLogger(dir.Path);
        logger.EnableFileLogging(true);

        logger.Warning("test message");
        logger.Dispose();

        var line = ReadLog(dir.Path)[0];
        // Format: 2026-03-16T14:22:31.123+00:00 [WARN] test message
        line.Should().MatchRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.*\[WARN\] test message$");
    }

    [Fact]
    public void EnableFileLogging_ToFalseAfterTrue_StopsAllWrites()
    {
        using var dir = new TempDirectory();
        using var logger = new FileAppLogger(dir.Path);
        logger.EnableFileLogging(true);
        logger.Warning("first");
        logger.EnableFileLogging(false);
        logger.Warning("second");
        logger.Dispose();

        var lines = ReadLog(dir.Path);
        lines.Should().HaveCount(1);
        lines[0].Should().Contain("first");
    }

    [Fact]
    public void Logger_AppendsToExistingFile()
    {
        using var dir = new TempDirectory();

        using (var first = new FileAppLogger(dir.Path))
        {
            first.EnableFileLogging(true);
            first.Warning("first session");
        }

        using (var second = new FileAppLogger(dir.Path))
        {
            second.EnableFileLogging(true);
            second.Warning("second session");
        }

        var lines = ReadLog(dir.Path);
        lines.Should().HaveCount(2);
        lines[0].Should().Contain("first session");
        lines[1].Should().Contain("second session");
    }

    [Fact]
    public void MethodCalls_AfterDispose_DoNotThrow()
    {
        using var dir = new TempDirectory();
        var logger = new FileAppLogger(dir.Path);
        logger.Dispose();

        var act = () =>
        {
            logger.Info("after dispose");
            logger.Warning("after dispose");
            logger.Error("after dispose");
            logger.EnableFileLogging(true);
        };

        act.Should().NotThrow();
    }

    private static string[] ReadLog(string directory)
    {
        var path = Path.Combine(directory, "pomodoro.log");
        if (!File.Exists(path)) return [];
        return File.ReadAllLines(path).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
    }
}

internal sealed class TempDirectory : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(
        System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

    public TempDirectory() => Directory.CreateDirectory(Path);

    public void Dispose()
    {
        if (Directory.Exists(Path))
            Directory.Delete(Path, recursive: true);
    }
}
