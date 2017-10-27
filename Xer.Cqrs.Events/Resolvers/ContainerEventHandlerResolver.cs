using System.Collections.Generic;
using System.Linq;

namespace Xer.Cqrs.Events.Resolvers
{
    public class ContainerEventHandlerResolver : IEventHandlerResolver
    {
        private readonly IContainerAdapter _containerAdapter;

        public ContainerEventHandlerResolver(IContainerAdapter containerAdapter)
        {
            _containerAdapter = containerAdapter;
        }

        public IEnumerable<EventHandlerDelegate> ResolveEventHandlers<TEvent>() where TEvent : IEvent
        {
            return buildEventHandlerDelegates<TEvent>();
        }

        private IEnumerable<EventHandlerDelegate> buildEventHandlerDelegates<TEvent>() where TEvent : IEvent
        {
            List<EventHandlerDelegate> handlerDelegates = new List<EventHandlerDelegate>();

            // Get all async handlers for the event.
            IEnumerable<IEventAsyncHandler<TEvent>> asyncEventHandlers = _containerAdapter.ResolveMultiple<IEventAsyncHandler<TEvent>>();

            if (asyncEventHandlers != null)
            {
                // Convert to EventHandlerDelegate.
                handlerDelegates.AddRange(asyncEventHandlers.Select(eventHandler =>
                {
                    return new EventHandlerDelegate((e, ct) =>
                    {
                        return eventHandler.HandleAsync((TEvent)e, ct);
                    });
                }));
            }

            // Get all sync handlers for the event.
            IEnumerable<IEventHandler<TEvent>> syncEventHandlers = _containerAdapter.ResolveMultiple<IEventHandler<TEvent>>();

            if (syncEventHandlers != null)
            {
               // Convert to EventHandlerDelegate.
               handlerDelegates.AddRange(syncEventHandlers.Select(eventHandler =>
               {
                   return new EventHandlerDelegate((e, ct) =>
                   {
                       eventHandler.Handle((TEvent)e);
                       return Utilities.CompletedTask;
                   });
               }));
            }

            // Merge the two.
            return handlerDelegates;
        }
    }

    public interface IContainerAdapter
    {
        IEnumerable<T> ResolveMultiple<T>() where T : class;
    }
}
