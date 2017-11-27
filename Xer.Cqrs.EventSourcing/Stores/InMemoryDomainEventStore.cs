using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Events;

namespace Xer.Cqrs.EventSourcing.Stores
{
    public class InMemoryDomainEventStore<TAggregate, TAggregateId> : IDomainEventStore<TAggregate, TAggregateId>, 
                                                                      IDomainEventAsyncStore<TAggregate, TAggregateId> 
                                                                      where TAggregate : IEventSourcedAggregate<TAggregateId>
                                                                      where TAggregateId : IEquatable<TAggregateId>
    {
        #region Declarations

        private readonly IDictionary<TAggregateId, DomainEventStream<TAggregateId>> _domainEventStreamsByAggregateId = new Dictionary<TAggregateId, DomainEventStream<TAggregateId>>();

        #endregion Declarations

        #region IDomainEventStore Implementation

        /// <summary>
        /// Get all domain events of aggregate.
        /// </summary>
        /// <param name="aggregateId">ID of the aggregate.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public virtual IDomainEventStream<TAggregateId> GetDomainEventStream(TAggregateId aggregateId)
        {
            return GetDomainEventStream(aggregateId, 1, int.MaxValue);
        }

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version.
        /// </summary>
        /// <param name="aggregateId">ID of the aggregate.</param>
        /// <param name="upToVersion">Target aggregate version.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public virtual IDomainEventStream<TAggregateId> GetDomainEventStream(TAggregateId aggregateId, int upToVersion)
        {
            return GetDomainEventStream(aggregateId, 1, upToVersion);
        }
        
        /// <summary>
        /// Get domain events of aggregate from the specified start and end version.
        /// </summary>
        /// <param name="aggregateId">ID of the aggregate.</param>
        /// <param name="fromVersion">Aggregate version to start retrieving domain events from.</param>
        /// <param name="toVersion">Target aggregate version.</param>
        /// <returns>Domain events for the aggregat with the specified version.</returns>
        public virtual IDomainEventStream<TAggregateId> GetDomainEventStream(TAggregateId aggregateId, int fromVersion, int toVersion)
        {
            DomainEventStream<TAggregateId> stream;

            if (!_domainEventStreamsByAggregateId.TryGetValue(aggregateId, out stream))
            {
                // Empty stream.
                return new DomainEventStream<TAggregateId>(aggregateId);
            }

            // Return a new copy, not the actual reference.
            return new DomainEventStream<TAggregateId>(aggregateId, stream.SkipWhile(e => e.AggregateVersion < fromVersion)
                                                                 .TakeWhile(e => e.AggregateVersion <= toVersion));
        }

        /// <summary>
        /// Persist aggregate to the event store.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate to persist.</param>
        public void Save(TAggregate aggregateRoot)
        {
            try
            {
                IDomainEventStream<TAggregateId> domainEventsToCommit = aggregateRoot.GetUncommitedDomainEvents();

                Commit(domainEventsToCommit);

                // Clear after committing and publishing.
                aggregateRoot.ClearUncommitedDomainEvents();
            }
            catch(Exception ex)
            {
                OnCommitError(ex);
            }
        }

        #endregion IDomainEventStore Implementation

        #region IDomainEventAsyncStore Implementation

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
        /// Get domain events of aggregate from the beginning up to the specified version asynchronously.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="upToVersion">Target aggregate version.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public virtual Task<IDomainEventStream<TAggregateId>> GetDomainEventStreamAsync(TAggregateId aggreggateId, int upToVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetDomainEventStreamAsync(aggreggateId, 1, upToVersion, cancellationToken);
        }

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version asynchronously.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="fromVersion">Aggregate version to start retrieving domain events from.</param>
        /// <param name="toVersion">Target aggregate version.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Domain events for the aggregate with the specified version.</returns>
        public virtual Task<IDomainEventStream<TAggregateId>> GetDomainEventStreamAsync(TAggregateId aggreggateId, int fromVersion, int toVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                IDomainEventStream<TAggregateId> stream = GetDomainEventStream(aggreggateId, fromVersion, toVersion);
                return Task.FromResult(stream);
            }
            catch (Exception ex)
            {
                return TaskUtility.FromException<IDomainEventStream<TAggregateId>>(ex);
            }
        }

        /// <summary>
        /// Persist aggregate to the event store asynchronously.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate to persist.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task which can be awaited asynchronously.</returns>
        public Task SaveAsync(TAggregate aggregateRoot, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                Save(aggregateRoot);
                return TaskUtility.CompletedTask;
            }
            catch(Exception ex)
            {
                return TaskUtility.FromException(ex);
            }
        }

        #endregion IDomainEventAsyncStore Implementation

        #region Protected Methods

        /// <summary>
        /// Commit the domain event to the in-memory store.
        /// </summary>
        /// <param name="domainEventStreamToCommit">Domain event to store.</param>
        protected virtual void Commit(IDomainEventStream<TAggregateId> domainEventStreamToCommit)
        {
            DomainEventStream<TAggregateId> existingStream;

            if (_domainEventStreamsByAggregateId.TryGetValue(domainEventStreamToCommit.AggregateId, out existingStream))
            {
                // Aggregate stream already exists.
                // Append and update.
                _domainEventStreamsByAggregateId[domainEventStreamToCommit.AggregateId] = existingStream.AppendDomainEventStream(domainEventStreamToCommit);
            }
            else
            {
                // Save.
                _domainEventStreamsByAggregateId.Add(domainEventStreamToCommit.AggregateId,
                    new DomainEventStream<TAggregateId>(domainEventStreamToCommit.AggregateId, domainEventStreamToCommit));
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

        #endregion Protected Methods
    }
}
