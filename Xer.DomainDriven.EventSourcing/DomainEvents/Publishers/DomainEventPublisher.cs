using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.DomainDriven.EventSourcing.DomainEvents.Publishers
{
    public class DomainEventPublisher : IDomainEventPublisher
    {
        private readonly IDomainEventSubscription _subscription;

        public DomainEventPublisher(IDomainEventSubscription subscription)
        {
            _subscription = subscription;
        }

        public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _subscription.NotifySubscribersAsync(domainEvent, cancellationToken);
        }
    }
}
