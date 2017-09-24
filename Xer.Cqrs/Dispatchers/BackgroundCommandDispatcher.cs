using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Registrations;

namespace Xer.Cqrs.Dispatchers
{
    public class BackgroundCommandDispatcher : ICommandDispatcher, ICommandAsyncDispatcher
    {
        private readonly ICommandHandlerProvider _provider;

        public BackgroundCommandDispatcher(ICommandHandlerProvider provider) 
        {
            _provider = provider;
        }

        /// <summary>
        /// Dispatch the command to the registered command handlers in the background.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        public void Dispatch(ICommand command)
        {
            DispatchAsync(command).PropagateAnyExceptions();
        }

        /// <summary>
        /// Dispatch the command to the registered command handlers in the background.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancellation.</param>
        /// <returns>Task which can be awaited asynchronously.</returns>
        public Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                Type comandType = command.GetType();

                CommandAsyncHandlerDelegate handleCommandAsyncDelegate = _provider.GetCommandHandler(comandType);

                return handleCommandAsyncDelegate.Invoke(command, cancellationToken);
            });
        }
    }
}
