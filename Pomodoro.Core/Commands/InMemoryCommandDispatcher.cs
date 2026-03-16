using Pomodoro.Core.Interfaces;

namespace Pomodoro.Core.Commands
{
    internal sealed class InMemoryCommandDispatcher : ICommandDispatcher
    {
        private readonly Dictionary<Type, Func<object, CancellationToken, Task<object>>> _handlers = [];

        public void RegisterHandler<TCommand, TResult>(ICommandHandler<TCommand, TResult> handler)
            where TCommand : ICommand<TResult>
            where TResult : notnull
        {
            _handlers[typeof(TCommand)] = async (command, cancellationToken) =>
                await handler.HandleAsync((TCommand)command, cancellationToken);
        }

        public Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
            where TResult : notnull
        {
            if (!_handlers.TryGetValue(command.GetType(), out var handler))
            {
                throw new InvalidOperationException($"No handler registered for {command.GetType().Name}.");
            }

            return DispatchInternalAsync<TResult>(handler, command, cancellationToken);
        }

        private static async Task<TResult> DispatchInternalAsync<TResult>(
            Func<object, CancellationToken, Task<object>> handler,
            object command,
            CancellationToken cancellationToken)
            where TResult : notnull
        {
            var result = await handler(command, cancellationToken);
            return (TResult)result;
        }
    }
}
