using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.EventStack
{
    /// <summary>
    /// Publishes events to subscribers.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Triggers when an exception occurs while handling events.
        /// </summary>
        event OnErrorHandler OnError;

        /// <summary>
        /// Publish event to registered event handlers.
        /// </summary>
        /// <param name="event">Event to publish.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancellation in event handlers.</param>
        /// <returns>Asynchronous task which completes when all the event handlers have processed the event.</returns>
        Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Publish events to registered event handlers.
        /// </summary>
        /// <param name="events">Events to publish.</param>
        /// <param name="cancellationToken">Optional cancellation token to support cancellation in event handlers.</param>
        /// <returns>Asynchronous task which completes when all the event handlers have processed the event.</returns>
        Task PublishAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default(CancellationToken));
    }

    public delegate void OnErrorHandler(IEvent @event, Exception e);
}
