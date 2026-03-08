using Pomodoro.Core.Commands.Settings;
using Pomodoro.Core.Common;

namespace Pomodoro.Core.Validation
{
    internal static class SettingsValidator
    {
        internal static Result ValidateTiming(SaveTimingSettingsCommand cmd)
        {
            if (cmd.WorkMinutes is < 1 or > 120)
                return Fail("Timing.WorkMinutes.OutOfRange", "Work duration must be between 1 and 120 minutes.");

            if (cmd.ShortBreakMinutes is < 1 or > 60)
                return Fail("Timing.ShortBreakMinutes.OutOfRange", "Short break must be between 1 and 60 minutes.");

            if (cmd.LongBreakMinutes is < 1 or > 60)
                return Fail("Timing.LongBreakMinutes.OutOfRange", "Long break must be between 1 and 60 minutes.");

            if (cmd.ShortBreakMinutes >= cmd.WorkMinutes)
                return Fail("Timing.ShortBreakMinutes.MustBeLessThanWork", "Short break must be shorter than the work duration.");

            if (cmd.LongBreakMinutes <= cmd.ShortBreakMinutes)
                return Fail("Timing.LongBreakMinutes.MustExceedShortBreak", "Long break must be longer than the short break.");

            if (cmd.LongBreakInterval is < 2 or > 10)
                return Fail("Timing.LongBreakInterval.OutOfRange", "Long break interval must be between 2 and 10 sessions.");

            if (cmd.MaxPhasePauseMinutes is < 1 or > 30)
                return Fail("Timing.MaxPhasePauseMinutes.OutOfRange", "Max pause duration must be between 1 and 30 minutes.");

            return Result.Success();
        }

        internal static Result ValidateProgression(SaveProgressionSettingsCommand cmd)
        {
            if (cmd.TargetWorkMinutes is < 1 or > 180)
                return Fail("Progression.TargetWorkMinutes.OutOfRange", "Target work duration must be between 1 and 180 minutes.");

            if (cmd.StepMinutes is < 1 or > 30)
                return Fail("Progression.StepMinutes.OutOfRange", "Step size must be between 1 and 30 minutes.");

            if (cmd.RequiredCompletionsToApplyStep is < 1 or > 100)
                return Fail("Progression.RequiredCompletions.OutOfRange", "Required completions must be between 1 and 100.");

            if (cmd.TargetWorkMinutes <= cmd.StepMinutes)
                return Fail("Progression.TargetWorkMinutes.MustExceedStep", "Target work duration must be greater than the step size.");

            return Result.Success();
        }

        private static Result Fail(string code, string message) =>
            Result.Failure(new Error(code, message));
    }
}
