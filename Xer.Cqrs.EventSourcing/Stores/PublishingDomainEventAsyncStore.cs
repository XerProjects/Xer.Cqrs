using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Events;

namespace Xer.Cqrs.EventSourcing.Stores
{
    public class PublishingDomainEventAsyncStore<TAggregate, TAggregateId> : IDomainEventAsyncStore<TAggregate, TAggregateId> 
                                                                             where TAggregate : IEventSourcedAggregate<TAggregateId>
                                                                             where TAggregateId : IEquatable<TAggregateId>
    {
        private readonly IDomainEventAsyncStore<TAggregate, TAggregateId> _domainEventStore;
        private readonly IEventPublisher _publisher;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domainEventStore">Decorated domain event store.</param>
        /// <param name="publisher">Event publisher.</param>
        public PublishingDomainEventAsyncStore(IDomainEventAsyncStore<TAggregate, TAggregateId> domainEventStore, IEventPublisher publisher)
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
        /// Get all domain events of aggregate asynchronously.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public Task<IDomainEventStream<TAggregateId>> GetDomainEventStreamAsync(TAggregateId aggreggateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _domainEventStore.GetDomainEventStreamAsync(aggreggateId, cancellationToken);
        }

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version asynchronously.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="upToVersion">Target aggregate version.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Domain events for the aggregate with the specified version.</returns>
        public Task<IDomainEventStream<TAggregateId>> GetDomainEventStreamAsync(TAggregateId aggreggateId, int upToVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _domainEventStore.GetDomainEventStreamAsync(aggreggateId, upToVersion, cancellationToken);
        }

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version asynchronously.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="fromVersion">Aggregate version to start retrieving domain events from.</param>
        /// <param name="toVersion">Target aggregate version.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Domain events for the aggregate with the specified version.</returns>
        public Task<IDomainEventStream<TAggregateId>> GetDomainEventStreamAsync(TAggregateId aggreggateId, int fromVersion, int toVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _domainEventStore.GetDomainEventStreamAsync(aggreggateId, fromVersion, toVersion,  cancellationToken);
        }

        public async Task SaveAsync(TAggregate aggregateRoot, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get a copy of the uncommited domain events before saving.
            IDomainEventStream<TAggregateId> domainEventStreamToSave = aggregateRoot.GetUncommitedDomainEvents();

            await _domainEventStore.SaveAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);

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
