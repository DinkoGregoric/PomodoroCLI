namespace Pomodoro.Core.Interfaces
{
    public interface ICommand<TResult>
        where TResult : notnull
    {
    }

    public interface ICommandHandler<TCommand, TResult>
        where TCommand : ICommand<TResult>
        where TResult : notnull
    {
        Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }

    public interface ICommandDispatcher
    {
        Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
            where TResult : notnull;
    }
}
