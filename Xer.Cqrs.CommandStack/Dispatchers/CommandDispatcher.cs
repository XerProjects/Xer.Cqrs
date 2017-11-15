using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher, ICommandAsyncDispatcher
    {
        private readonly ICommandHandlerResolver _resolver;

        public CommandDispatcher(ICommandHandlerResolver resolver)
        {
            _resolver = resolver;
        }

        /// <summary>
        /// Dispatch the command to the registered command handler.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to dispatch.</typeparam>
        /// <param name="command">Command to dispatch.</param>
        public void Dispatch<TCommand>(TCommand command) where TCommand : class, ICommand
        {
            // Wait Task completion.
            DispatchAsync(command).Await();
        }

        /// <summary>
        /// Dispatch the command to the registered command handler asynchronously.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to dispatch.</typeparam>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancellation in command handlers.</param>
        /// <returns>Asynchronous task which completes after the command handler has processed the command.</returns>
        public Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, ICommand
        {
            if(command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            CommandHandlerDelegate handleCommandAsyncDelegate = _resolver.ResolveCommandHandler<TCommand>();

            if(handleCommandAsyncDelegate == null)
            {
                throw new CommandNotHandledException($"No command handler is registered to handle command of type: {typeof(TCommand).Name}.");
            }
            
            return handleCommandAsyncDelegate.Invoke(command, cancellationToken);
        }
    }
}
