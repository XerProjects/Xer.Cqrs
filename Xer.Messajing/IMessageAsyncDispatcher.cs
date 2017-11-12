using System.Threading;
using System.Threading.Tasks;

namespace Xer.Messajing
{
    public interface IMessageAsyncDispatcher
    {
        /// <summary>
        /// Dispatch the message to the registered message handlers asynchronously.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to dispatch.</typeparam>
        /// <param name="message">Message to dispatch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        Task DispatchAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default(CancellationToken)) where TMessage : IMessage;

        /// <summary>
        /// Dispatch the message to the registered message handlers 
        /// and return message processing result asynchronously.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to dispatch.</typeparam>
        /// <typeparam name="TResult">Type of result of message processing.</typeparam>
        /// <param name="message">Message to dispatch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task with result of message processing.</returns>
        Task<TResult> DispatchAsync<TMessage, TResult>(TMessage message, CancellationToken cancellationToken = default(CancellationToken)) where TMessage : IMessage<TResult>;
    }
}
