using System;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack;

namespace Xer.EventSourcing.Stores
{
    public class PublishingDomainEventStore<TAggregate, TAggregateId> : IDomainEventStore<TAggregate, TAggregateId> 
                                                                        where TAggregate : IEventSourcedAggregate<TAggregateId>
                                                                        where TAggregateId : IEquatable<TAggregateId>
    {
        private readonly IDomainEventStore<TAggregate, TAggregateId> _domainEventStore;
        private readonly IEventPublisher _publisher;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domainEventStore">Decorated domain event store.</param>
        /// <param name="publisher">Event publisher.</param>
        public PublishingDomainEventStore(IDomainEventStore<TAggregate, TAggregateId> domainEventStore, IEventPublisher publisher)
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
        public IDomainEventStream<TAggregateId> GetDomainEventStream(TAggregateId aggreggateId)
        {
            return _domainEventStore.GetDomainEventStream(aggreggateId);
        }

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="upToVersion">Target aggregate version.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public IDomainEventStream<TAggregateId> GetDomainEventStream(TAggregateId aggreggateId, int upToVersion)
        {
            return _domainEventStore.GetDomainEventStream(aggreggateId, upToVersion);
        }

        /// <summary>
        /// Get domain events of aggregate from the specified start and end version.
        /// </summary>
        /// <param name="aggregateId">ID of the aggregate.</param>
        /// <param name="fromVersion">Aggregate version to start retrieving domain events from.</param>
        /// <param name="toVersion">Target aggregate version.</param>
        /// <returns>Domain events for the aggregat with the specified version.</returns>
        public IDomainEventStream<TAggregateId> GetDomainEventStream(TAggregateId aggregateId, int fromVersion, int toVersion)
        {
            return _domainEventStore.GetDomainEventStream(aggregateId, fromVersion, toVersion);
        }

        /// <summary>
        /// Persist aggregate to the event store and publish the events.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate to persist.</param>
        public void Save(TAggregate aggregateRoot)
        {
            // Get a copy of the uncommited domain events before saving.
            IDomainEventStream<TAggregateId> domainEventStreamToSave = aggregateRoot.GetUncommitedDomainEvents();

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
        protected virtual Task PublishDomainEventsAsync(IDomainEventStream<TAggregateId> eventStream)
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
