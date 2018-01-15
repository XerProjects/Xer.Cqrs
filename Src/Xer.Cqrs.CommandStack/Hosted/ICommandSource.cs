using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.CommandStack.Hosted
{
    public interface ICommandSource : ICommandSource<object>
    {
    }

    public interface ICommandSource<TCommand> where TCommand : class
    {   
        /// <summary>
        /// Received that is triggered when a command is received.
        /// </summary>
        event CommandReceivedDelegate<TCommand> CommandReceived;

        /// <summary>
        /// Start receiving commands from the source.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task that can be awaited for completion.</returns>
        Task StartReceiving(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Stop receiving commands from the source.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task that can be awaited for completion.</returns>
        Task StopReceiving(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Manually receive command.
        /// </summary>
        /// <param name="command">Command to receive.</param>
        /// <returns>Asynchronous task that can be awaited for completion.</returns>
        Task Receive(TCommand command);
    }
}