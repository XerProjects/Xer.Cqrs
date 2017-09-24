using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs
{
    public interface ICommandAsyncDispatcher
    {
        /// <summary>
        /// Dispatch the command to the registered command handlers asynchronously.
        /// </summary>
        /// <param name="command">Command to dispatch.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancellation.</param>
        /// <returns>Task which can be awaited asynchronously.</returns>
        Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default(CancellationToken));
    }
}
