using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs
{
    public interface ICommandAsyncHandler<TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// Handle and process command asynchronously.
        /// </summary>
        /// <param name="command">Command to handle and process.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancellation.</param>
        /// <returns>Task which can be awaited asynchronously.</returns>
        Task HandleAsync(TCommand command, CancellationToken cancellationToken = default(CancellationToken));
    }
}
