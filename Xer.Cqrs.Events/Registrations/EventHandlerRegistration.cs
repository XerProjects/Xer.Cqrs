using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xer.Cqrs.Events.Registrations
{
    public class EventHandlerRegistration : IEventHandlerRegistration, IEventHandlerResolver
    {
        private readonly EventHandlerDelegateCollectionStore _eventHandlerDelegateStore = new EventHandlerDelegateCollectionStore();

        public void Register<TEvent>(Func<IEventAsyncHandler<TEvent>> eventHandlerFactory) where TEvent : IEvent
        {
            Type eventType = typeof(TEvent);

            EventHandlerDelegate newSubscribedEventHandlerDelegate = (e, ct) =>
            {
                IEventAsyncHandler<TEvent> instance;

                try
                {
                    instance = eventHandlerFactory.Invoke();
                    if (instance == null)
                    {
                        throw new InvalidOperationException("Unable to resolve an instance of IEventAsyncHandler from the registered factory.");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Unable to resolve an instance of IEventAsyncHandler from the registered factory.", ex);
                }
                
                return instance.HandleAsync((TEvent)e, ct);
            };

            _eventHandlerDelegateStore.Add(eventType, newSubscribedEventHandlerDelegate);
        }

        public void Register<TEvent>(Func<IEventHandler<TEvent>> eventHandlerFactory) where TEvent : IEvent
        {
            Type eventType = typeof(TEvent);

            EventHandlerDelegate newSubscribedEventHandlerDelegate = (domainEvent, ct) =>
            {
                IEventHandler<TEvent> instance;

                try
                {
                    instance = eventHandlerFactory.Invoke();
                    if (instance == null)
                    {
                        throw new InvalidOperationException("Unable to resolve an instance of IEventHandler from the registered factory.");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Unable to resolve an instance of IEventHandler from the registered factory.", ex);
                }
                
                instance.Handle((TEvent)domainEvent);

                return Utilities.CompletedTask;
            };

            _eventHandlerDelegateStore.Add(eventType, newSubscribedEventHandlerDelegate);
        }

        public IEnumerable<EventHandlerDelegate> ResolveEventHandlers<TEvent>() where TEvent : IEvent
        {
            Type eventType = typeof(TEvent);

            IEnumerable<EventHandlerDelegate> eventHandlerDelegates;
            _eventHandlerDelegateStore.TryGetEventHandlerDelegates(eventType, out eventHandlerDelegates);

            return eventHandlerDelegates;
        }
    }
}
