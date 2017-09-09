using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.EventSourcing.DomainEvents.Subscriptions
{
    public class DomainEventSubscription : IDomainEventSubscription
    {
        private readonly Dictionary<Type, List<Action<IDomainEvent>>> _subscriberActionsByDomainEventType = new Dictionary<Type, List<Action<IDomainEvent>>>();

        public void NotifySubscribers<TTopic>(TTopic domainEvent) where TTopic : IDomainEvent
        {
            Type domainEventType = domainEvent.GetType();

            List<Action<IDomainEvent>> subscriberActions;
            if (_subscriberActionsByDomainEventType.TryGetValue(domainEventType, out subscriberActions))
            {
                foreach (var handler in subscriberActions)
                {
                    handler.Invoke(domainEvent);
                }
            }
        }

        public void Subscribe<TTopic>(IDomainEventSubscriber<TTopic> subscriber) where TTopic : IDomainEvent
        {
            Type topicType = typeof(TTopic);

            List<Action<IDomainEvent>> handlers;
            if (_subscriberActionsByDomainEventType.TryGetValue(topicType, out handlers))
            {
                _subscriberActionsByDomainEventType[topicType].Add(new Action<IDomainEvent>((domainEvent) => subscriber.Handle((TTopic)domainEvent)));
            }
            else
            {
                _subscriberActionsByDomainEventType.Add(topicType, new List<Action<IDomainEvent>>
                {
                    new Action<IDomainEvent>((domainEvent) => subscriber.Handle((TTopic)domainEvent))
                });
            }
        }
    }
}
