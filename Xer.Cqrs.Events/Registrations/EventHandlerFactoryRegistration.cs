using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xer.Cqrs.Events.Registrations
{
    public class EventHandlerFactoryRegistration : IEventHandlerFactoryRegistration, IEventHandlerResolver
    {
        private static readonly Task _completedTask = Task.FromResult(0);
        private static readonly EventHandlerDelegate _defaultEventHandlerDelegate = new EventHandlerDelegate((e, ct) => _completedTask);

        private readonly IDictionary<Type, EventHandlerDelegate> _eventHandlerDelegateByEventType = new Dictionary<Type, EventHandlerDelegate>();

        public void Register<TEvent>(Func<IEventAsyncHandler<TEvent>> eventSubscriberFactory) where TEvent : IEvent
        {
            Type topicType = typeof(TEvent);

            EventHandlerDelegate newSubscribedEventHandlerDelegate = (domainEvent, ct) =>
            {
                IEventAsyncHandler<TEvent> instance;

                try
                {
                    instance = eventSubscriberFactory.Invoke();
                    if (instance == null)
                    {
                        throw new InvalidOperationException("Unable to resolve an instance of IEventAsyncHandler from the registered factory.");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Unable to resolve an instance of IEventAsyncHandler from the registered factory.", ex);
                }
                
                return instance.HandleAsync((TEvent)domainEvent, ct);
            };

            EventHandlerDelegate eventHandlerDelegate;
            if (_eventHandlerDelegateByEventType.TryGetValue(topicType, out eventHandlerDelegate))
            {
                eventHandlerDelegate += newSubscribedEventHandlerDelegate;
            }
            else
            {
                _eventHandlerDelegateByEventType.Add(topicType, newSubscribedEventHandlerDelegate);
            }
        }

        public void Register<TEvent>(Func<IEventHandler<TEvent>> eventSubscriberFactory) where TEvent : IEvent
        {
            Type topicType = typeof(TEvent);

            EventHandlerDelegate newSubscribedEventHandlerDelegate = (domainEvent, ct) =>
            {
                IEventHandler<TEvent> instance;

                try
                {
                    instance = eventSubscriberFactory.Invoke();
                    if (instance == null)
                    {
                        throw new InvalidOperationException("Unable to resolve an instance of IEventHandler from the registered factory.");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Unable to resolve an instance of IEventHandler from the registered factory.", ex);
                }

                try
                {
                    instance.Handle((TEvent)domainEvent);

                    return _completedTask;
                }
                catch (Exception)
                {
                    throw;
                }
            };

            EventHandlerDelegate eventHandlerDelegate;
            if (_eventHandlerDelegateByEventType.TryGetValue(topicType, out eventHandlerDelegate))
            {
                eventHandlerDelegate += newSubscribedEventHandlerDelegate;
            }
            else
            {
                _eventHandlerDelegateByEventType.Add(topicType, newSubscribedEventHandlerDelegate);
            }
        }

        public EventHandlerDelegate ResolveEventHandler<TEvent>() where TEvent : IEvent
        {
            Type eventType = typeof(TEvent);

            EventHandlerDelegate eventHandlerDelegate;
            if (_eventHandlerDelegateByEventType.TryGetValue(eventType, out eventHandlerDelegate))
            {
                return eventHandlerDelegate;
            }

            // No subscribed handlers. Return default which does nothing.
            return _defaultEventHandlerDelegate;
        }
    }
}
