using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.EventSourcing.DomainEvents.Subscriptions
{
    public class DomainEventSubscription : IDomainEventSubscription
    {
        private Dictionary<Type, List<Action<IDomainEvent>>> _handlerByDomainEventType = new Dictionary<Type, List<Action<IDomainEvent>>>();

        public void NotifySubscribers<TTopic>(TTopic domainEvent) where TTopic : IDomainEvent
        {
            Type domainEventType = domainEvent.GetType();

            List<Action<IDomainEvent>> handlers;
            if (_handlerByDomainEventType.TryGetValue(domainEventType, out handlers))
            {
                foreach (var handler in handlers)
                {
                    handler.Invoke(domainEvent);
                }
            }
        }

        public void Subscribe<TTopic>(IDomainEventSubscriber<TTopic> subscriber) where TTopic : IDomainEvent
        {
            Type topicType = typeof(TTopic);

            List<Action<IDomainEvent>> handlers;
            if (_handlerByDomainEventType.TryGetValue(topicType, out handlers))
            {
                _handlerByDomainEventType[topicType].Add(new Action<IDomainEvent>((domainEvent) => subscriber.Handle((TTopic)domainEvent)));
            }
            else
            {
                _handlerByDomainEventType.Add(topicType, new List<Action<IDomainEvent>>
                {
                    new Action<IDomainEvent>((domainEvent) => subscriber.Handle((TTopic)domainEvent))
                });
            }
        }
    }
}
