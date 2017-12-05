using System.Collections.Generic;
using System.Linq;

namespace Xer.Cqrs.EventStack.Resolvers
{
    public class CompositeEventHandlerResolver : IEventHandlerResolver
    {
        private readonly IEnumerable<IEventHandlerResolver> _eventHandlerResolvers;

        public CompositeEventHandlerResolver(IEnumerable<IEventHandlerResolver> eventHandlerResolvers)
        {
            _eventHandlerResolvers = eventHandlerResolvers;
        }

        /// <summary>
        /// Get registered event handler delegates which handle the event of the specified type from multiple sources.
        /// </summary>
        /// <typeparam name="TEvent">Type of event to be handled.</typeparam>
        /// <returns>Collection of <see cref="EventHandlerDelegate"/> which executes event handler processing.</returns>
        public IEnumerable<EventHandlerDelegate> ResolveEventHandlers<TEvent>() where TEvent : class, IEvent
        {
            return _eventHandlerResolvers.SelectMany(e => e.ResolveEventHandlers<TEvent>());
        }
    }
}
