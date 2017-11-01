using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Events;

namespace Xer.Cqrs.EventSourcing.DomainEvents.Stores
{
    public class InMemoryDomainEventStore<TAggregate> : IDomainEventStore<TAggregate>, IDomainEventAsyncStore<TAggregate> where TAggregate : EventSourcedAggregate
    {
        #region Declarations

        private readonly IDictionary<Guid, DomainEventStream> _domainEventStreamsByAggregateId = new Dictionary<Guid, DomainEventStream>();

        #endregion Declarations

        #region IDomainEventStore Implementation

        /// <summary>
        /// Get all domain events of aggregate.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public virtual DomainEventStream GetDomainEventStream(Guid aggreggateId)
        {
            DomainEventStream stream;

            if (!_domainEventStreamsByAggregateId.TryGetValue(aggreggateId, out stream))
            {
                stream = DomainEventStream.Empty;
            }

            // Return a new copy, not the actual reference.
            return new DomainEventStream(stream.AggregateId, stream);
        }

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="version">Target aggregate version.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public virtual DomainEventStream GetDomainEventStream(Guid aggreggateId, int version)
        {
            DomainEventStream stream;

            if (!_domainEventStreamsByAggregateId.TryGetValue(aggreggateId, out stream))
            {
                stream = DomainEventStream.Empty;
            }

            // Return a new copy, not the actual reference.
            return new DomainEventStream(stream.AggregateId, stream.TakeWhile(e => e.AggregateVersion <= version));
        }

        /// <summary>
        /// Persist aggregate to the event store.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate to persist.</param>
        public void Save(TAggregate aggregateRoot)
        {
            try
            {
                DomainEventStream domainEventsToCommit = aggregateRoot.GetUncommitedDomainEvents();

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
        public virtual Task<DomainEventStream> GetDomainEventStreamAsync(Guid aggreggateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                DomainEventStream stream = GetDomainEventStream(aggreggateId);
                return Task.FromResult(stream);
            }
            catch (Exception ex)
            {
                return TaskUtility.FromException<DomainEventStream>(ex);
            }
        }

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version asynchronously.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="version">Target aggregate version.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public virtual Task<DomainEventStream> GetDomainEventStreamAsync(Guid aggreggateId, int version, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                DomainEventStream stream = GetDomainEventStream(aggreggateId, version);
                return Task.FromResult(stream);
            }
            catch (Exception ex)
            {
                return TaskUtility.FromException<DomainEventStream>(ex);
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
        protected virtual void Commit(DomainEventStream domainEventStreamToCommit)
        {
            DomainEventStream existingStream;

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
                    new DomainEventStream(domainEventStreamToCommit.AggregateId, domainEventStreamToCommit));
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
