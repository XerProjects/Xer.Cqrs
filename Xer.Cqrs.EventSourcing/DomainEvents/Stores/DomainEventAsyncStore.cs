using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Events;

namespace Xer.Cqrs.EventSourcing.DomainEvents.Stores
{
    public abstract class DomainEventAsyncStore<TAggregate> : IDomainEventAsyncStore<TAggregate> where TAggregate : EventSourcedAggregate
    {
        private readonly IEventPublisher _publisher;

        public DomainEventAsyncStore(IEventPublisher publisher)
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
            try
            {
                DomainEventStream domainEventsToCommit = aggregateRoot.GetUncommitedDomainEvents();
                await CommitAsync(domainEventsToCommit, cancellationToken).ConfigureAwait(false);
                PublishDomainEventsAsync(domainEventsToCommit, cancellationToken);
                // Clear after committing and publishing.
                aggregateRoot.ClearUncommitedDomainEvents();
            }
            catch(Exception ex)
            {
                OnCommitError(ex);
            }
        }

        /// <summary>
        /// Publishes the domain event to event subscribers asynchronously.
        /// Default implementation publishes domain events in background.
        /// </summary>
        /// <param name="eventStream">Domain events to publish.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        protected virtual void PublishDomainEventsAsync(DomainEventStream eventStream, CancellationToken cancellationToken = default(CancellationToken))
        {
            IEnumerable<Task> publishTasks = eventStream.Select(e => _publisher.PublishAsync(e, cancellationToken));

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

        /// <summary>
        /// Provide child class to handle exceptions that occur while committing.
        /// </summary>
        /// <param name="ex">Exception that occured while publishing domain events.</param>
        protected virtual void OnCommitError(Exception ex)
        {
            throw ex;
        }
    }
}
