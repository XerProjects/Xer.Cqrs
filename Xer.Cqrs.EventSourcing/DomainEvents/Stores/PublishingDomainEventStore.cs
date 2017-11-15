using System;
using System.Threading.Tasks;
using Xer.Cqrs.Events;

namespace Xer.Cqrs.EventSourcing.DomainEvents.Stores
{
    public class PublishingDomainEventStore<TAggregate> : IDomainEventStore<TAggregate> where TAggregate : IEventSourcedAggregate
    {
        private readonly IDomainEventStore<TAggregate> _domainEventStore;
        private readonly IEventPublisher _publisher;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domainEventStore">Decorated domain event store.</param>
        /// <param name="publisher">Event publisher.</param>
        public PublishingDomainEventStore(IDomainEventStore<TAggregate> domainEventStore, IEventPublisher publisher)
        {
            _domainEventStore = domainEventStore;
            _publisher = publisher;

            // Subscribe to any errors in publishing.
            _publisher.OnError += (e, ex) =>
            {
                OnPublishError((IDomainEvent)e, ex);
            };
        }

        /// <summary>
        /// Get all domain events of aggregate.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public DomainEventStream GetDomainEventStream(Guid aggreggateId)
        {
            return _domainEventStore.GetDomainEventStream(aggreggateId);
        }

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="version">Target aggregate version.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public DomainEventStream GetDomainEventStream(Guid aggreggateId, int version)
        {
            return _domainEventStore.GetDomainEventStream(aggreggateId, version);
        }

        /// <summary>
        /// Persist aggregate to the event store and publish the events.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate to persist.</param>
        public void Save(TAggregate aggregateRoot)
        {
            // Get a copy of the uncommited domain events before saving.
            DomainEventStream domainEventStreamToSave = aggregateRoot.GetUncommitedDomainEvents();

            _domainEventStore.Save(aggregateRoot);

            // No need to await. Any publish errors will be communicated through OnError event.
            // Not passing cancellation token since event notification should not be cancelled.
            Task publishTask = PublishDomainEventsAsync(domainEventStreamToSave);
        }

        /// <summary>
        /// Publishes the domain event to event subscribers.
        /// Default implementation publishes domain events in background.
        /// </summary>
        /// <param name="eventStream">Domain events to publish.</param>
        /// <returns>Asynchronous task.</returns>
        protected virtual Task PublishDomainEventsAsync(DomainEventStream eventStream)
        {
            return _publisher.PublishAsync(eventStream);
        }

        /// <summary>
        /// Provide child class to handle exceptions that occur while publishing.
        /// </summary>
        /// <param name="domainEvent">Domain event.</param>
        /// <param name="ex">Exception that occured while publishing domain events.</param>
        protected virtual void OnPublishError(IDomainEvent domainEvent, Exception ex)
        {
            // Do not throw exceptions from this method.
        }
    }
}
