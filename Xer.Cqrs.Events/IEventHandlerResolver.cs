using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.Events
{
    public interface IEventHandlerResolver
    {
        /// <summary>
        /// Get the registered command handler delegate to handle the event of the specified type.
        /// </summary>
        /// <typeparam name="TEvent">Type of event to be handled.</typeparam>
        /// <returns>Collection of event handlers that are registered for the event.</returns>
        IEnumerable<EventHandlerDelegate> ResolveEventHandlers<TEvent>() where TEvent : class, IEvent;
    }

    /// <summary>
    /// Delegate to handle event. This can contain actions for multiple event handlers.
    /// </summary>
    /// <param name="event">Event to handle.</param>
    public delegate Task EventHandlerDelegate(IEvent @event, CancellationToken cancellationToken = default(CancellationToken));
}
