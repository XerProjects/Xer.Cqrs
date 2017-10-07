using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack.Dispatchers
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
            DispatchAsync(command);
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
                Type commandType = command.GetType();

                CommandHandlerDelegate commandHandlerDelegate = _provider.GetCommandHandler(commandType);

                return commandHandlerDelegate.Invoke(command, cancellationToken);
            });
        }
    }
}
