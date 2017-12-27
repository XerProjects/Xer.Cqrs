using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.EventStack.Hosted
{
    public abstract class HostedEventHandler<TEvent> : IEventAsyncHandler<TEvent>, 
                                                       IEventHandler<TEvent> 
                                                       where TEvent : class, IEvent
    {
        /// <summary>
        /// Internal event source derived from EventSource.
        /// </summary>
        private IEventSource _internalEventSource;

        /// <summary>
        /// Event source where event handler will subscribe to for events.
        /// </summary>
        protected abstract IEventSource EventSource { get; }

        /// <summary>
        /// Start hostec event handler.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Completed asynchronous task.</returns>
        public virtual Task Start(CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get an instance provided by child class and store in a private property. 
            _internalEventSource = EventSource;
            
            if(_internalEventSource == null)
            {
                throw new InvalidOperationException("Hosted event handler has no event source.");
            }

            OnStart();

            // Subscribe.
            _internalEventSource.EventReceived += (receivedEvent, ct) =>
            {
                TEvent @event = receivedEvent as TEvent;
                if(@event != null)
                {
                    ProcessEventAsync(@event, ct);
                }

                return TaskUtility.CompletedTask;
            };

            _internalEventSource.StartReceiving(cancellationToken);

            return TaskUtility.CompletedTask;
        }

        /// <summary>
        /// Stop hosted event handler.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task which can be awaited until the last received event has finished processing.</returns>
        public virtual Task Stop(CancellationToken cancellationToken = default(CancellationToken))
        {
            OnStop();
            
            return _internalEventSource.StopReceiving(cancellationToken);
        }

        /// <summary>
        /// Process event asynchronously.
        /// </summary>
        /// <remarks>
        /// It is recommended to not let any exceptions exit this method because
        /// any exceptions that exits this method will not be handled.
        /// </remarks>
        /// <param name="receivedEvent">Event received.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        protected abstract Task ProcessEventAsync(TEvent receivedEvent, CancellationToken cancellationToken);

        /// <summary>
        /// Handle event by putting it to the event source for asynchronous processing.
        /// </summary>
        /// <param name="@event">Event to handle.</param>
        /// <param name="cancellationToken">Cancellation token. This is ignored.</param>
        /// <returns>Completed task.</returns>
        Task IEventAsyncHandler<TEvent>.HandleAsync(TEvent @event, CancellationToken cancellationToken)
        {
            return _internalEventSource.Receive(@event);
        }

        /// <summary>
        /// Handle event by putting it to the event source for asynchronous processing.
        /// </summary>
        /// <param name="@event">Event to handle.</param>
        void IEventHandler<TEvent>.Handle(TEvent @event)
        {
            _internalEventSource.Receive(@event).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Hook that is executed before hosted event handler is started.
        /// </summary>
        protected virtual void OnStart()
        {
        }
        
        /// <summary>
        /// Hook that is executed before hosted event handler is stopped.
        /// </summary>
        protected virtual void OnStop()
        {
        }
    }
}