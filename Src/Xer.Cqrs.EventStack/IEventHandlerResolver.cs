using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.EventStack
{
    public interface IEventHandlerResolver
    {
        /// <summary>
        /// Get registered event handler delegates which handle the event of the specified type.
        /// </summary>
        /// <typeparam name="TEvent">Type of event to be handled.</typeparam>
        /// <returns>Collection of <see cref="EventHandlerDelegate"/> which executes event handler processing.</returns>
        IEnumerable<EventHandlerDelegate> ResolveEventHandlers<TEvent>() where TEvent : class, IEvent;
    }

    /// <summary>
    /// Delegate to handle event.
    /// </summary>
    /// <param name="event">Event to handle.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Asynchronous task which completes after the event handler has processed the event.</returns>
    public delegate Task EventHandlerDelegate(IEvent @event, CancellationToken cancellationToken = default(CancellationToken));
}
