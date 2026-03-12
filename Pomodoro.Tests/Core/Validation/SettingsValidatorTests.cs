using AwesomeAssertions;
using Pomodoro.Core.Commands.Settings;
using Pomodoro.Core.Validation;

namespace Pomodoro.Tests.Core.Validation;

public class SettingsValidatorTests
{
    // --- ValidateTiming ---

    [Theory]
    [InlineData(0,   5,  15,  4,  5, "Timing.WorkMinutes.OutOfRange")]
    [InlineData(121, 5,  15,  4,  5, "Timing.WorkMinutes.OutOfRange")]
    [InlineData(25,  0,  15,  4,  5, "Timing.ShortBreakMinutes.OutOfRange")]
    [InlineData(25,  61, 15,  4,  5, "Timing.ShortBreakMinutes.OutOfRange")]
    [InlineData(25,  5,  0,   4,  5, "Timing.LongBreakMinutes.OutOfRange")]
    [InlineData(25,  5,  61,  4,  5, "Timing.LongBreakMinutes.OutOfRange")]
    [InlineData(25,  25, 30,  4,  5, "Timing.ShortBreakMinutes.MustBeLessThanWork")]
    [InlineData(15,  20, 25,  4,  5, "Timing.ShortBreakMinutes.MustBeLessThanWork")]
    [InlineData(25,  10, 10,  4,  5, "Timing.LongBreakMinutes.MustExceedShortBreak")]
    [InlineData(25,  10, 5,   4,  5, "Timing.LongBreakMinutes.MustExceedShortBreak")]
    [InlineData(25,  5,  15,  1,  5, "Timing.LongBreakInterval.OutOfRange")]
    [InlineData(25,  5,  15,  11, 5, "Timing.LongBreakInterval.OutOfRange")]
    [InlineData(25,  5,  15,  4,  0, "Timing.MaxPhasePauseMinutes.OutOfRange")]
    [InlineData(25,  5,  15,  4,  31,"Timing.MaxPhasePauseMinutes.OutOfRange")]
    public void ValidateTiming_InvalidInput_ReturnsExpectedError(
        int workMinutes, int shortBreak, int longBreak, int longBreakInterval, int maxPauseMinutes, string expectedCode)
    {
        var result = SettingsValidator.ValidateTiming(
            new SaveTimingSettingsCommand(workMinutes, shortBreak, longBreak, longBreakInterval, maxPauseMinutes));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(expectedCode);
    }

    [Fact]
    public void ValidateTiming_ValidDefaults_Succeeds()
    {
        var result = SettingsValidator.ValidateTiming(new SaveTimingSettingsCommand(25, 5, 15, 4, 5));

        result.IsSuccess.Should().BeTrue();
    }

    // --- ValidateProgression ---

    [Theory]
    [InlineData(false, 0,   5,  10, "Progression.TargetWorkMinutes.OutOfRange")]
    [InlineData(false, 181, 5,  10, "Progression.TargetWorkMinutes.OutOfRange")]
    [InlineData(false, 45,  0,  10, "Progression.StepMinutes.OutOfRange")]
    [InlineData(false, 45,  31, 10, "Progression.StepMinutes.OutOfRange")]
    [InlineData(false, 45,  5,  0,  "Progression.RequiredCompletions.OutOfRange")]
    [InlineData(false, 45,  5,  101,"Progression.RequiredCompletions.OutOfRange")]
    [InlineData(false, 5,   5,  10, "Progression.TargetWorkMinutes.MustExceedStep")]
    [InlineData(false, 4,   5,  10, "Progression.TargetWorkMinutes.MustExceedStep")]
    public void ValidateProgression_InvalidInput_ReturnsExpectedError(
        bool progressionEnabled, int target, int step, int completions, string expectedCode)
    {
        var result = SettingsValidator.ValidateProgression(
            new SaveProgressionSettingsCommand(progressionEnabled, target, step, completions));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(expectedCode);
    }

    [Fact]
    public void ValidateProgression_ValidDefaults_Succeeds()
    {
        var result = SettingsValidator.ValidateProgression(new SaveProgressionSettingsCommand(false, 45, 5, 10));

        result.IsSuccess.Should().BeTrue();
    }
}
