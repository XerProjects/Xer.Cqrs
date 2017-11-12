using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Messajing.Dispatchers
{
    public class BackgroundMessageDispatcher : IMessageDispatcher, IMessageAsyncDispatcher
    {
        private readonly MessageDispatcher _innerDispatcher;

        public BackgroundMessageDispatcher(MessageDispatcher innerDispatcher)
        {
            _innerDispatcher = innerDispatcher;
        }

        public void Dispatch<TMessage>(TMessage message) where TMessage : IMessage
        {
            DispatchAsync(message).ContinueWith(t => t.Await());
        }

        public Task DispatchAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default(CancellationToken)) where TMessage : IMessage
        {
            return Task.Run(() =>
            {
                return _innerDispatcher.DispatchAsync(message);
            });
        }

        public TResult Dispatch<TMessage, TResult>(TMessage message) where TMessage : IMessage<TResult>
        {
            return _innerDispatcher.Dispatch<TMessage, TResult>(message);
        }

        public Task<TResult> DispatchAsync<TMessage, TResult>(TMessage message, CancellationToken cancellationToken = default(CancellationToken)) where TMessage : IMessage<TResult>
        {
            return _innerDispatcher.DispatchAsync<TMessage, TResult>(message, cancellationToken);
        }
    }
}
