using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.Events
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
        /// Publish event to subscribers.
        /// </summary>
        /// <param name="event">Event to publish.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default(CancellationToken));
    }

    public delegate void OnErrorHandler(IEvent @event, Exception e);
}
