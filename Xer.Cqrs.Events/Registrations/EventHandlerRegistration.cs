using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xer.Cqrs.Events.Registrations
{
    public class EventHandlerRegistration : IEventHandlerRegistration, IEventHandlerResolver
    {
        #region Declarations

        private readonly EventHandlerDelegateCollectionStore _eventHandlerDelegateStore = new EventHandlerDelegateCollectionStore();

        #endregion Declarations

        #region IEventHandlerRegistration Implementation

        /// <summary>
        /// Register event handler as subscriber.
        /// </summary>
        /// <typeparam name="TEvent">Event to subscribe to.</typeparam>
        /// <param name="eventHandlerFactory">Event handler instance factory.</param>
        public void Register<TEvent>(Func<IEventAsyncHandler<TEvent>> eventHandlerFactory) where TEvent : class, IEvent
        {
            if(eventHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerFactory));
            }

            Type eventType = typeof(TEvent);

            EventHandlerDelegate newSubscribedEventHandlerDelegate = EventHandlerDelegateBuilder.FromFactory(eventHandlerFactory);

            _eventHandlerDelegateStore.Add(eventType, newSubscribedEventHandlerDelegate);
        }

        /// <summary>
        /// Register event handler as subscriber.
        /// </summary>
        /// <typeparam name="TEvent">Event to subscribe to.</typeparam>
        /// <param name="eventHandlerFactory">Event async handler instance factory.</param>
        public void Register<TEvent>(Func<IEventHandler<TEvent>> eventHandlerFactory) where TEvent : class, IEvent
        {
            if (eventHandlerFactory == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerFactory));
            }

            Type eventType = typeof(TEvent);

            EventHandlerDelegate newSubscribedEventHandlerDelegate = EventHandlerDelegateBuilder.FromFactory(eventHandlerFactory);

            _eventHandlerDelegateStore.Add(eventType, newSubscribedEventHandlerDelegate);
        }

        #endregion IEventHandlerRegistration Implementation

        #region IEventHandlerResolver Implementation

        /// <summary>
        /// Get the registered command handler delegate to handle the event of the specified type.
        /// </summary>
        /// <typeparam name="TEvent">Type of event to be handled.</typeparam>
        /// <returns>Collection of event handlers that are registered for the event.</returns>
        public IEnumerable<EventHandlerDelegate> ResolveEventHandlers<TEvent>() where TEvent : class, IEvent
        {
            Type eventType = typeof(TEvent);

            IEnumerable<EventHandlerDelegate> eventHandlerDelegates;
            _eventHandlerDelegateStore.TryGetEventHandlerDelegates(eventType, out eventHandlerDelegates);

            return eventHandlerDelegates;
        }

        #endregion IEventHandlerResolver Implementation
    }
}
