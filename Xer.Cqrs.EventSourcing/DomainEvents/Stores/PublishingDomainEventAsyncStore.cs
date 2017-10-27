using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Events;

namespace Xer.Cqrs.EventSourcing.DomainEvents.Stores
{
    public class PublishingDomainEventAsyncStore<TAggregate> : IDomainEventAsyncStore<TAggregate> where TAggregate : EventSourcedAggregate
    {
        private readonly IDomainEventAsyncStore<TAggregate> _domainEventStore;
        private readonly IEventPublisher _publisher;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domainEventStore">Decorated domain event store.</param>
        /// <param name="publisher">Event publisher.</param>
        public PublishingDomainEventAsyncStore(IDomainEventAsyncStore<TAggregate> domainEventStore, IEventPublisher publisher)
        {
            _domainEventStore = domainEventStore;
            _publisher = publisher;
        }

        public Task<DomainEventStream> GetDomainEventStreamAsync(Guid aggreggateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _domainEventStore.GetDomainEventStreamAsync(aggreggateId, cancellationToken);
        }

        public Task<DomainEventStream> GetDomainEventStreamAsync(Guid aggreggateId, int version, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _domainEventStore.GetDomainEventStreamAsync(aggreggateId, version, cancellationToken);
        }

        public async Task SaveAsync(TAggregate aggregateRoot, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get a copy of the uncommited domain events before saving.
            DomainEventStream domainEventStreamToSave = aggregateRoot.GetUncommitedDomainEvents();

             await _domainEventStore.SaveAsync(aggregateRoot, cancellationToken).ConfigureAwait(false);

            try
            {
                PublishDomainEventsAsync(domainEventStreamToSave);
            }
            catch (Exception ex)
            {
                OnPublishError(ex);
            }
        }

        /// <summary>
        /// Publishes the domain event to event subscribers.
        /// Default implementation publishes domain events in background.
        /// </summary>
        /// <param name="eventStream">Domain events to publish.</param>
        protected virtual void PublishDomainEventsAsync(DomainEventStream eventStream)
        {
            IEnumerable<Task> publishTasks = eventStream.Select(e => _publisher.PublishAsync(e));

            Task.WhenAll(publishTasks)
            .HandleAnyExceptions(ex =>
            {
                OnPublishError(ex);
            });
        }

        /// <summary>
        /// Provide child class to handle exceptions that occur while publishing.
        /// </summary>
        /// <param name="ex">Exception that occured while publishing domain events.</param>
        protected virtual void OnPublishError(Exception ex)
        {
            throw ex;
        }
    }
}
