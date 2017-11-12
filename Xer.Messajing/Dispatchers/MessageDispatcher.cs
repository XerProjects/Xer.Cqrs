using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Messajing
{
    public class MessageDispatcher : IMessageDispatcher, IMessageAsyncDispatcher
    {
        private readonly IMessageHandlerDelegateResolver _resolver;

        public MessageDispatcher(IMessageHandlerDelegateResolver resolver)
        {
            _resolver = resolver;
        }

        /// <summary>
        /// Dispatch the message to the registered message handlers.
        /// </summary>
        /// <param name="message">Message to dispatch.</param>
        public void Dispatch<TMessage>(TMessage message) where TMessage : IMessage
        {
            // Wait Task completion.
            DispatchAsync(message).Await();
        }

        /// <summary>
        /// Dispatch the message to the registered message handlers asynchronously.
        /// </summary>
        /// <param name="message">Message to dispatch.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancellation.</param>
        /// <returns>Task which can be awaited asynchronously.</returns>
        public Task DispatchAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default(CancellationToken)) where TMessage : IMessage
        {
            Type commandType = message.GetType();

            MessageHandlerDelegate messageHandlerDelegate = _resolver.Resolve<TMessage>();

            return messageHandlerDelegate.Invoke(message, cancellationToken);
        }

        /// <summary>
        /// Dispatch the message to the registered message handlers 
        /// and return message processing result.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to dispatch.</typeparam>
        /// <typeparam name="TResult">Type of result of message processing.</typeparam>
        /// <param name="message">Message to dispatch.</param>
        /// <returns>Result of message processing.</returns>
        public TResult Dispatch<TMessage, TResult>(TMessage message) where TMessage : IMessage<TResult>
        {
            return DispatchAsync<TMessage, TResult>(message).Await();
        }

        /// <summary>
        /// Dispatch the message to the registered message handlers 
        /// and return message processing result asynchronously.
        /// </summary>
        /// <typeparam name="TMessage">Type of message to dispatch.</typeparam>
        /// <typeparam name="TResult">Type of result of message processing.</typeparam>
        /// <param name="message">Message to dispatch.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task with result of message processing.</returns>
        public Task<TResult> DispatchAsync<TMessage, TResult>(TMessage message, CancellationToken cancellationToken = default(CancellationToken)) where TMessage : IMessage<TResult>
        {
            Type commandType = message.GetType();

            MessageHandlerDelegate<TResult> messageHandlerDelegate = _resolver.Resolve<TMessage, TResult>();

            return messageHandlerDelegate.Invoke(message, cancellationToken);
        }
    }
}
