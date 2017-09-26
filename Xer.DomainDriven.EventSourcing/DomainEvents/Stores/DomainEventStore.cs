using System;
using System.Collections.Generic;
using Xer.DomainDriven.EventSourcing.DomainEvents.Publishers;

namespace Xer.DomainDriven.EventSourcing.DomainEvents.Stores
{
    public abstract class DomainEventStore<TAggregate> : IDomainEventStore<TAggregate> where TAggregate : EventSourcedAggregate
    {
        private readonly IDomainEventPublisher _publisher;

        public DomainEventStore(IDomainEventPublisher publisher)
        {
            _publisher = publisher;
        }

        /// <summary>
        /// Get all domain events of aggregate.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public abstract DomainEventStream GetDomainEventStream(Guid aggreggateId);

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="version">Target aggregate version.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public abstract DomainEventStream GetDomainEventStream(Guid aggreggateId, int version);

        /// <summary>
        /// Commit the domain event to the store.
        /// </summary>
        /// <param name="domainEventStreamToCommit">Domain event to store.</param>
        protected abstract void Commit(DomainEventStream domainEventStreamToCommit);

        /// <summary>
        /// Persist aggregate to the event store.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate to persist.</param>
        public void Save(TAggregate aggregateRoot)
        {
            DomainEventStream domainEventsToCommit = aggregateRoot.GetUncommitedDomainEvents();
            
            Commit(domainEventsToCommit);

            foreach (IDomainEvent commitedEvent in domainEventsToCommit)
            {
                NotifySubscribers(commitedEvent);
            }

            // Clear after committing and publishing.
            aggregateRoot.ClearUncommitedDomainEvents();
        }

        /// <summary>
        /// Publishes the domain event to event subscribers.
        /// </summary>
        /// <param name="domainEvent">Domain event to publish.</param>
        private void NotifySubscribers(IDomainEvent domainEvent)
        {
            _publisher.PublishAsync(domainEvent).ContinueWith(async t => await t);
        }
    }
}
