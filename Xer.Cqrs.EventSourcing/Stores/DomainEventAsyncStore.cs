using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Events;

namespace Xer.Cqrs.EventSourcing.Stores
{
    public abstract class DomainEventAsyncStore<TAggregate, TAggregateId> : IDomainEventAsyncStore<TAggregate, TAggregateId> 
                                                                            where TAggregate : IEventSourcedAggregate<TAggregateId>
                                                                            where TAggregateId : IEquatable<TAggregateId>
    {
        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version asynchronously.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="fromVersion">Aggregate version to start retrieving domain events from.</param>
        /// <param name="toVersion">Target aggregate version.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Domain events for the aggregate with the specified version.</returns>
        public abstract Task<IDomainEventStream<TAggregateId>> GetDomainEventStreamAsync(TAggregateId aggreggateId, int fromVersion, int toVersion, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version asynchronously.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="upToVersion">Target aggregate version.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Domain events for the aggregate with the specified version.</returns>
        public virtual Task<IDomainEventStream<TAggregateId>> GetDomainEventStreamAsync(TAggregateId aggreggateId, int upToVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetDomainEventStreamAsync(aggreggateId, 1, int.MaxValue, cancellationToken);
        }

        /// <summary>
        /// Get all domain events of aggregate asynchronously.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public virtual Task<IDomainEventStream<TAggregateId>> GetDomainEventStreamAsync(TAggregateId aggreggateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetDomainEventStreamAsync(aggreggateId, 1, int.MaxValue, cancellationToken);
        }

        /// <summary>
        /// Commit the domain event to the store asynchronously.
        /// </summary>
        /// <param name="domainEventStreamToCommit">Domain event to store.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task which can be awaited asynchronously.</returns>
        protected abstract Task CommitAsync(IDomainEventStream<TAggregateId> domainEventStreamToCommit, CancellationToken cancellationToken = default(CancellationToken));

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
                // Get uncommited events.
                IDomainEventStream<TAggregateId> domainEventsToCommit = aggregateRoot.GetUncommitedDomainEvents();

                await CommitAsync(domainEventsToCommit, cancellationToken).ConfigureAwait(false);

                // Clear after committing and publishing.
                aggregateRoot.ClearUncommitedDomainEvents();
            }
            catch(Exception ex)
            {
                OnCommitError(ex);
            }
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
