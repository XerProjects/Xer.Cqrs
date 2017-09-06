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

        public abstract DomainEventStream GetDomainEventStream(Guid aggreggateId);
        public abstract IReadOnlyCollection<DomainEventStream> GetAllDomainEventStreams();

        public void Save(TAggregate aggregateRoot)
        {
            DomainEventStream domainEventsToCommit = aggregateRoot.FlushUncommitedDomainEvents();
            
            Commit(domainEventsToCommit);

            foreach (IDomainEvent commitedEvent in domainEventsToCommit)
            {
                NotifySubscribers(commitedEvent);
            }

            // Clear after committing and publishing.
            // aggregateRoot.ClearUncommitedDomainEventStream();
        }

        /// <summary>
        /// Commit the domain event to the store.
        /// </summary>
        /// <param name="domainEventStreamToCommit">Domain event to store.</param>
        /// <returns>True, if domain event has been successfully committed.</returns>
        protected abstract void Commit(DomainEventStream domainEventStreamToCommit);

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
