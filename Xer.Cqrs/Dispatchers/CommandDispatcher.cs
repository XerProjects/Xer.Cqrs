using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Registrations;

namespace Xer.Cqrs.Dispatchers
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
            Type comandType = command.GetType();

            CommandAsyncHandlerDelegate handleCommandAsyncDelegate = _provider.GetCommandHandler(comandType);
            
            return handleCommandAsyncDelegate.Invoke(command, cancellationToken);
        }
    }
}
