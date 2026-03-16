using AwesomeAssertions;
using Pomodoro.Infrastructure;

namespace Pomodoro.Tests.Infrastructure;

public class SettingsProviderTests
{
    [Fact]
    public async Task LoadSettingsAsync_FileNotFound_ReturnsDefaultSettings()
    {
        using var dir = new TempDirectory();
        using var provider = new SettingsProvider(dir.Path);

        var result = await provider.LoadSettingsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Timing.WorkMinutes.Should().Be(25);
        result.Value.Timing.ShortBreakMinutes.Should().Be(5);
        result.Value.Timing.LongBreakMinutes.Should().Be(15);
        result.Value.Timing.LongBreakInterval.Should().Be(4);
    }

    [Fact]
    public async Task LoadSettingsAsync_FileNotFound_CreatesFileOnDisk()
    {
        using var dir = new TempDirectory();
        using var provider = new SettingsProvider(dir.Path);

        await provider.LoadSettingsAsync();

        File.Exists(Path.Combine(dir.Path, "settings.json")).Should().BeTrue();
    }

    [Fact]
    public async Task LoadSettingsAsync_ValidJson_ReturnsDeserializedValues()
    {
        using var dir = new TempDirectory();
        var json = """
            {
              "Timing": { "WorkMinutes": 30 },
              "Progression": {},
              "Notifications": {},
              "Diagnostics": {},
              "Info": {}
            }
            """;
        await File.WriteAllTextAsync(Path.Combine(dir.Path, "settings.json"), json, TestContext.Current.CancellationToken);

        using var provider = new SettingsProvider(dir.Path);
        var result = await provider.LoadSettingsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Timing.WorkMinutes.Should().Be(30);
    }

    [Fact]
    public async Task LoadSettingsAsync_CorruptedJson_ReturnsDefaultsAndSuccess()
    {
        using var dir = new TempDirectory();
        await File.WriteAllTextAsync(Path.Combine(dir.Path, "settings.json"), "{invalid json", TestContext.Current.CancellationToken);

        using var provider = new SettingsProvider(dir.Path);
        var result = await provider.LoadSettingsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Timing.WorkMinutes.Should().Be(25);
    }

    [Fact]
    public async Task LoadSettingsAsync_NullJson_ReturnsDefaults()
    {
        using var dir = new TempDirectory();
        await File.WriteAllTextAsync(Path.Combine(dir.Path, "settings.json"), "null", TestContext.Current.CancellationToken);

        using var provider = new SettingsProvider(dir.Path);
        var result = await provider.LoadSettingsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Timing.WorkMinutes.Should().Be(25);
    }

    [Fact]
    public async Task SaveSettingsAsync_ValidSettings_RoundTripsCorrectly()
    {
        using var dir = new TempDirectory();
        using var provider = new SettingsProvider(dir.Path);

        var loadResult = await provider.LoadSettingsAsync();
        loadResult.Value.Timing.WorkMinutes = 45;
        await provider.SaveSettingsAsync(loadResult.Value);

        var reloadResult = await provider.LoadSettingsAsync();

        reloadResult.IsSuccess.Should().BeTrue();
        reloadResult.Value.Timing.WorkMinutes.Should().Be(45);
    }

    [Fact]
    public async Task SaveSettingsAsync_DirectoryMissing_CreatesDirectoryAndFile()
    {
        using var dir = new TempDirectory();
        var subDir = Path.Combine(dir.Path, "nested", "config");
        using var provider = new SettingsProvider(subDir);

        var settings = new Pomodoro.Core.Domain.PomodoroSettings();
        var result = await provider.SaveSettingsAsync(settings);

        result.IsSuccess.Should().BeTrue();
        File.Exists(Path.Combine(subDir, "settings.json")).Should().BeTrue();
    }
}
