using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xer.DomainDriven.EventSourcing.DomainEvents.Publishers;

namespace Xer.DomainDriven.EventSourcing.DomainEvents.Stores
{
    public abstract class DomainEventAsyncStore<TAggregate> : IDomainEventAsyncStore<TAggregate> where TAggregate : EventSourcedAggregate
    {
        private readonly IDomainEventPublisher _publisher;

        public DomainEventAsyncStore(IDomainEventPublisher publisher)
        {
            _publisher = publisher;
        }

        /// <summary>
        /// Get all domain events of aggregate asynchronously.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public abstract Task<DomainEventStream> GetDomainEventStreamAsync(Guid aggreggateId, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version asynchronously.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="version">Target aggregate version.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public abstract Task<DomainEventStream> GetDomainEventStreamAsync(Guid aggreggateId, int version, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Commit the domain event to the store asynchronously.
        /// </summary>
        /// <param name="domainEventStreamToCommit">Domain event to store.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task which can be awaited asynchronously.</returns>
        protected abstract Task CommitAsync(DomainEventStream domainEventStreamToCommit, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Persist aggregate to the event store asynchronously.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate to persist.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task which can be awaited asynchronously.</returns>
        public async Task SaveAsync(TAggregate aggregateRoot, CancellationToken cancellationToken = default(CancellationToken))
        {
            DomainEventStream domainEventsToCommit = aggregateRoot.GetUncommitedDomainEvents();

            await CommitAsync(domainEventsToCommit, cancellationToken).ConfigureAwait(false);

            await Task.WhenAll(domainEventsToCommit.Select(d => NotifySubscribersAsync(d))).ConfigureAwait(false);

            // Clear after committing and publishing.
            aggregateRoot.ClearUncommitedDomainEvents();
        }

        /// <summary>
        /// Publishes the domain event to event subscribers asynchronously.
        /// </summary>
        /// <param name="domainEvent">Domain event to publish.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task which can be awaited asynchronously.</returns>
        private Task NotifySubscribersAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _publisher.PublishAsync(domainEvent);
        }
    }
}
