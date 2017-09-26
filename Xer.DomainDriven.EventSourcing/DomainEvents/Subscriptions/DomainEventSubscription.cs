using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.DomainDriven.EventSourcing.DomainEvents.Subscriptions
{
    public class DomainEventSubscription : IDomainEventSubscription
    {
        private delegate Task NotifySubscriberAsyncDelegate(IDomainEvent domainEvent, CancellationToken cancellationToken = default(CancellationToken));

        private readonly IDictionary<Type, NotifySubscriberAsyncDelegate> _subscriberActionsByDomainEventType = new Dictionary<Type, NotifySubscriberAsyncDelegate>();

        public void Subscribe<TTopic>(IDomainEventAsyncHandler<TTopic> eventSubscriber) where TTopic : IDomainEvent
        {
            Type topicType = typeof(TTopic);

            NotifySubscriberAsyncDelegate notifySubscriberDelegate = (domainEvent, ct) =>
            {
                return eventSubscriber.HandleAsync((TTopic)domainEvent, ct);
            };

            NotifySubscriberAsyncDelegate notifySubscriberDelegates;
            if (_subscriberActionsByDomainEventType.TryGetValue(topicType, out notifySubscriberDelegates))
            {
                notifySubscriberDelegates += notifySubscriberDelegate;
            }
            else
            {
                _subscriberActionsByDomainEventType.Add(topicType, notifySubscriberDelegate);
            }
        }

        public void Subscribe<TTopic>(IDomainEventHandler<TTopic> eventSubscriber) where TTopic : IDomainEvent
        {
            Type topicType = typeof(TTopic);

            NotifySubscriberAsyncDelegate notifySubscriberDelegate = (domainEvent, ct) =>
            {
                eventSubscriber.Handle((TTopic)domainEvent);

                return TaskUtility.CompletedTask;
            };

            NotifySubscriberAsyncDelegate notifySubscriberDelegates;
            if (_subscriberActionsByDomainEventType.TryGetValue(topicType, out notifySubscriberDelegates))
            {
                notifySubscriberDelegates += notifySubscriberDelegate;
            }
            else
            {
                _subscriberActionsByDomainEventType.Add(topicType, notifySubscriberDelegate);
            }
        }

        public Task NotifySubscribersAsync<TTopic>(TTopic domainEvent, CancellationToken cancellationToken = default(CancellationToken)) where TTopic : IDomainEvent
        {
            Type topicType = domainEvent.GetType();

            NotifySubscriberAsyncDelegate notifySubscriberDelegates;
            if (_subscriberActionsByDomainEventType.TryGetValue(topicType, out notifySubscriberDelegates))
            {
                return notifySubscriberDelegates.Invoke(domainEvent, cancellationToken);
            }

            return TaskUtility.CompletedTask;
        }
    }
}
