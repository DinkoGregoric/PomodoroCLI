using AwesomeAssertions;
using Pomodoro.Core.Commands;
using Pomodoro.Core.Common;
using Pomodoro.Core.Interfaces;

namespace Pomodoro.Tests.Core.Commands;

public class InMemoryCommandDispatcherTests
{
    private record FakeCommand : ICommand<Result>;
    private record AnotherFakeCommand : ICommand<Result>;

    private class FakeHandler(Result result) : ICommandHandler<FakeCommand, Result>
    {
        public int CallCount { get; private set; }
        public CancellationToken? ReceivedToken { get; private set; }

        public Task<Result> HandleAsync(FakeCommand command, CancellationToken cancellationToken = default)
        {
            CallCount++;
            ReceivedToken = cancellationToken;
            return Task.FromResult(result);
        }
    }

    private class AnotherFakeHandler(Result result) : ICommandHandler<AnotherFakeCommand, Result>
    {
        public Task<Result> HandleAsync(AnotherFakeCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }

    [Fact]
    public async Task Dispatch_RegisteredCommand_InvokesHandlerAndReturnsResult()
    {
        var dispatcher = new InMemoryCommandDispatcher();
        var handler = new FakeHandler(Result.Success());
        dispatcher.RegisterHandler(handler);

        var result = await dispatcher.DispatchAsync(new FakeCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        handler.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task Dispatch_UnregisteredCommand_ThrowsInvalidOperationException()
    {
        var dispatcher = new InMemoryCommandDispatcher();

        var act = async () => await dispatcher.DispatchAsync(new FakeCommand());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*FakeCommand*");
    }

    [Fact]
    public async Task Dispatch_MultipleCommandTypes_RoutesEachToCorrectHandler()
    {
        var dispatcher = new InMemoryCommandDispatcher();
        var fakeHandler = new FakeHandler(Result.Success());
        var anotherHandler = new AnotherFakeHandler(Result.Failure(new Error("Another.Error", "Another error.")));
        dispatcher.RegisterHandler(fakeHandler);
        dispatcher.RegisterHandler(anotherHandler);

        var result1 = await dispatcher.DispatchAsync(new FakeCommand(), TestContext.Current.CancellationToken);
        var result2 = await dispatcher.DispatchAsync(new AnotherFakeCommand(), TestContext.Current.CancellationToken);

        result1.IsSuccess.Should().BeTrue();
        result2.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterHandler_Overwrite_SecondHandlerIsInvoked()
    {
        var dispatcher = new InMemoryCommandDispatcher();
        var first = new FakeHandler(Result.Failure(new Error("First", "First handler.")));
        var second = new FakeHandler(Result.Success());
        dispatcher.RegisterHandler(first);
        dispatcher.RegisterHandler(second);

        var result = await dispatcher.DispatchAsync(new FakeCommand(), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        first.CallCount.Should().Be(0);
        second.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task Dispatch_PassesCancellationToken_ToHandler()
    {
        var dispatcher = new InMemoryCommandDispatcher();
        var handler = new FakeHandler(Result.Success());
        dispatcher.RegisterHandler(handler);

        using var cts = new CancellationTokenSource();
        await dispatcher.DispatchAsync(new FakeCommand(), cts.Token);

        handler.ReceivedToken.Should().Be(cts.Token);
    }
}
