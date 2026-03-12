namespace Pomodoro.Tests.Core.Engine;

internal sealed class FakeTimeProvider(DateTimeOffset startTime) : TimeProvider
{
    private DateTimeOffset _now = startTime;

    public void Advance(TimeSpan by) => _now += by;

    public override DateTimeOffset GetUtcNow() => _now;
}
