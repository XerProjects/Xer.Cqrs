using System.Threading;
using System.Threading.Tasks;

namespace Xer.Messajing
{
    public interface IMessageAsyncHandler<in TMessage> where TMessage : IMessage
    {
        /// <summary>
        /// Handle message processing asynchronously.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        Task HandleAsync(TMessage message, CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IMessageAsyncHandler<in TMessage, TResult> where TMessage : IMessage<TResult>
    {
        /// <summary>
        /// Handle message processing and return message processing result asynchronously.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task with message processing result.</returns>
        Task<TResult> HandleAsync(TMessage message, CancellationToken cancellationToken = default(CancellationToken));
    }
}
