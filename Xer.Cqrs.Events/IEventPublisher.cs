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
        /// Publish event to subscribers.
        /// </summary>
        /// <param name="event">Event to publish.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Asynchronous task.</returns>
        Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default(CancellationToken));
    }
}
