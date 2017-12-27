using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack
{
    public interface ICommandAsyncDispatcher
    {
        /// <summary>
        /// Dispatch the command to the registered command handler asynchronously.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to dispatch.</typeparam>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancellation in command handlers.</param>
        /// <returns>Asynchronous task which completes after the command handler has processed the command.</returns>
        Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, ICommand;
    }
}
