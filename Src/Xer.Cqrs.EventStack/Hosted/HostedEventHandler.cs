using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Messaginator;

namespace Xer.Cqrs.EventStack.Hosted
{
    public abstract class HostedEventHandler : HostedEventHandler<object>
    {
    }

    public abstract class HostedEventHandler<TEvent> : MessageProcessor<TEvent>,
                                                       IEventAsyncHandler<TEvent>, 
                                                       IEventHandler<TEvent> 
                                                       where TEvent : class
    {
        /// <summary>
        /// Handle event by putting it to the event source for asynchronous processing.
        /// </summary>
        /// <param name="@event">Event to handle.</param>
        /// <param name="cancellationToken">Cancellation token. This is ignored.</param>
        /// <returns>Completed task.</returns>
        Task IEventAsyncHandler<TEvent>.HandleAsync(TEvent @event, CancellationToken cancellationToken)
        {
            return MessageSource.ReceiveAsync(@event);
        }

        /// <summary>
        /// Handle event by putting it to the event source for asynchronous processing.
        /// </summary>
        /// <param name="@event">Event to handle.</param>
        void IEventHandler<TEvent>.Handle(TEvent @event)
        {
            MessageSource.ReceiveAsync(@event).GetAwaiter().GetResult();
        }
    }
}