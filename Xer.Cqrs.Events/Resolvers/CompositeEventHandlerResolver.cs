using System.Collections.Generic;
using System.Linq;

namespace Xer.Cqrs.Events.Resolvers
{
    public class CompositeEventHandlerResolver : IEventHandlerResolver
    {
        private readonly IEnumerable<IEventHandlerResolver> _eventHandlerResolvers;

        public CompositeEventHandlerResolver(IEnumerable<IEventHandlerResolver> eventHandlerResolvers)
        {
            _eventHandlerResolvers = eventHandlerResolvers;
        }

        public IEnumerable<EventHandlerDelegate> ResolveEventHandlers<TEvent>() where TEvent : class, IEvent
        {
            return _eventHandlerResolvers.SelectMany(e => e.ResolveEventHandlers<TEvent>());
        }
    }
}
