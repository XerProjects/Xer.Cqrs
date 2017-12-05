using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Xer.Cqrs.EventStack.Resolvers
{
    public class ContainerEventHandlerResolver : IEventHandlerResolver
    {
        private readonly IContainerAdapter _containerAdapter;

        public ContainerEventHandlerResolver(IContainerAdapter containerAdapter)
        {
            _containerAdapter = containerAdapter;
        }

        /// <summary>
        /// <para>Resolve instances of IEventAsyncHandler<TEvent> and IEventHandler<TEvent> which handles the given event type</para>
        /// <para>from the container and convert to a list of event handler delegates which invokes event handlers.</para>
        /// </summary>
        /// <typeparam name="TEvent">Type of event which is handled by the event handlers to resolve.</typeparam>
        /// <returns>Collection of <see cref="EventHandlerDelegate"/> which executes event handler processing.</returns>
        public IEnumerable<EventHandlerDelegate> ResolveEventHandlers<TEvent>() where TEvent : class, IEvent
        {
            return buildEventHandlerDelegates<TEvent>();
        }

        private IEnumerable<EventHandlerDelegate> buildEventHandlerDelegates<TEvent>() where TEvent : class, IEvent
        {
            List<EventHandlerDelegate> handlerDelegates = new List<EventHandlerDelegate>();

            try
            {
                // Get all async handlers for the event.
                IEnumerable<IEventAsyncHandler<TEvent>> asyncEventHandlers = _containerAdapter.ResolveMultiple<IEventAsyncHandler<TEvent>>();

                if (asyncEventHandlers != null)
                {
                    // Convert to EventHandlerDelegate.
                    handlerDelegates.AddRange(asyncEventHandlers.Select(eventHandler =>
                    {
                        return EventHandlerDelegateBuilder.FromEventHandler(eventHandler);
                    }));
                }
            }
            catch(Exception)
            {
                // Do nothing.
                // Some containers may throw exception when no instance is resolved.
            }

            try
            {
                // Get all sync handlers for the event.
                IEnumerable<IEventHandler<TEvent>> syncEventHandlers = _containerAdapter.ResolveMultiple<IEventHandler<TEvent>>();

                if (syncEventHandlers != null)
                {
                    // Convert to EventHandlerDelegate.
                    handlerDelegates.AddRange(syncEventHandlers.Select(eventHandler =>
                    {
                        return EventHandlerDelegateBuilder.FromEventHandler(eventHandler);
                    }));
                }
            }
            catch(Exception)
            {
                // Do nothing.
                // Some containers may throw exception when no instance is resolved.
            }
            
            return new ReadOnlyCollection<EventHandlerDelegate>(handlerDelegates);
        }
    }

    public interface IContainerAdapter
    {
        IEnumerable<T> ResolveMultiple<T>() where T : class;
    }
}
