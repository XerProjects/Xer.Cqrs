using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Messaginator;

namespace Xer.Cqrs.EventStack
{
    public abstract class HostedEventHandler : HostedEventHandler<object>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messageSource">Source of message.</param>
        public HostedEventHandler(IMessageSource<object> messageSource) : base(messageSource)
        {
        }
    }

    public abstract class HostedEventHandler<TEvent> : MessageProcessor<TEvent>,
                                                       IEventAsyncHandler<TEvent>, 
                                                       IEventHandler<TEvent> 
                                                       where TEvent : class
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messageSource">Source of message.</param>
        public HostedEventHandler(IMessageSource<TEvent> messageSource) : base(messageSource)
        {
        }

        /// <summary>
        /// Handle event by putting it to the event source for asynchronous processing.
        /// </summary>
        /// <param name="@event">Event to handle.</param>
        /// <param name="cancellationToken">Cancellation token. This is ignored.</param>
        /// <returns>Completed task.</returns>
        Task IEventAsyncHandler<TEvent>.HandleAsync(TEvent @event, CancellationToken cancellationToken)
        {
            return MessageSource.ReceiveAsync(new MessageContainer<TEvent>(@event), cancellationToken);
        }

        /// <summary>
        /// Handle event by putting it to the event source for asynchronous processing.
        /// </summary>
        /// <param name="@event">Event to handle.</param>
        void IEventHandler<TEvent>.Handle(TEvent @event)
        {
            MessageSource.ReceiveAsync(new MessageContainer<TEvent>(@event)).GetAwaiter().GetResult();
        }
    }
}