using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack
{
    public interface ICommandAsyncDispatcher
    {
        /// <summary>
        /// Dispatch the command to the registered command handlers asynchronously.
        /// </summary>
        /// <typeparam name="TCommand">Type of command to dispatch.</typeparam>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancellation.</param>
        /// <returns>Task which can be awaited asynchronously.</returns>
        Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, ICommand;
    }
}
