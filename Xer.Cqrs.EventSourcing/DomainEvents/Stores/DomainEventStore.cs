using Xer.Cqrs.EventSourcing.DomainEvents.Publishers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Xer.Cqrs.EventSourcing.DomainEvents.Stores
{
    public abstract class DomainEventStore<TAggregate> : IDomainEventStore<TAggregate> where TAggregate : EventSourcedAggregateRoot
    {
        private readonly DomainEventPublisher _publisher;

        public DomainEventStore(DomainEventPublisher publisher)
        {
            _publisher = publisher;
        }

        public abstract IReadOnlyCollection<IDomainEvent> GetDomainEventStream(Guid aggreggateId);
        public abstract ILookup<Guid, IReadOnlyCollection<IDomainEvent>> GetAllDomainEventStreamsGroupedById();

        public void Save(TAggregate aggregateRoot)
        {
            var domainEventsToCommit = aggregateRoot.GetUncommittedDomainEvents();

            foreach (IDomainEvent domainEvent in domainEventsToCommit)
            {
                if (Commit(domainEvent))
                {
                    NotifySubscribers(domainEvent);
                }
            }

            // Clear after committing and publishing.
            aggregateRoot.ClearUncommitedDomainEvents();
        }

        /// <summary>
        /// Commit the domain event to the store.
        /// </summary>
        /// <param name="domainEventToCommit">Domain event to store.</param>
        /// <returns>True, if domain event has been successfully committed.</returns>
        protected abstract bool Commit(IDomainEvent domainEventToCommit);

        /// <summary>
        /// Publishes the domain event to event subscribers.
        /// </summary>
        /// <param name="domainEvent">Domain event to publish.</param>
        private void NotifySubscribers(IDomainEvent domainEvent)
        {
            _publisher.Publish(domainEvent);
        }
    }
}
