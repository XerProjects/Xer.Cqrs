using System;
using System.Collections.Generic;
using System.Linq;

namespace Xer.DomainDriven.EventSourcing.DomainEvents.Publishers
{
    public class DomainEventPublisher : IDomainEventPublisher
    {
        protected IReadOnlyList<IDomainEventSubscription> Subscriptions { get; }

        public DomainEventPublisher(IEnumerable<IDomainEventSubscription> subscriptions)
        {
            Subscriptions = subscriptions.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(subscriptions));
        }
        
        public virtual void Publish(IDomainEvent domainEvent)
        {
            foreach (var subscription in Subscriptions)
            {
                subscription.NotifySubscribers(domainEvent);
            }
        }
    }
}
