using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher, ICommandAsyncDispatcher
    {
        private readonly ICommandHandlerProvider _provider;

        public CommandDispatcher(ICommandHandlerProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Dispatch the command to the registered command handlers.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        public void Dispatch(ICommand command)
        {
            // Wait Task completion.
            DispatchAsync(command).Await();
        }

        /// <summary>
        /// Dispatch the command to the registered command handlers asynchronously.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancellation.</param>
        /// <returns>Task which can be awaited asynchronously.</returns>
        public Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            Type commandType = command.GetType();

            CommandHandlerDelegate handleCommandAsyncDelegate = _provider.GetCommandHandler(commandType);
            
            return handleCommandAsyncDelegate.Invoke(command, cancellationToken);
        }
    }
}
