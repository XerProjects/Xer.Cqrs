using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xer.Cqrs.Events;

namespace Xer.Cqrs.EventSourcing.Stores
{
    public abstract class DomainEventStore<TAggregate, TAggregateId> : IDomainEventStore<TAggregate, TAggregateId> 
                                                                       where TAggregate : IEventSourcedAggregate<TAggregateId>
                                                                       where TAggregateId : IEquatable<TAggregateId>
    {
        /// <summary>
        /// Get domain events of aggregate from the specified start and end version.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="fromVersion">Aggregate version to start retrieving domain events from.</param>
        /// <param name="toVersion">Target aggregate version.</param>
        /// <returns>Domain events for the aggregat with the specified version.</returns>
        public abstract IDomainEventStream<TAggregateId> GetDomainEventStream(TAggregateId aggreggateId, int fromVersion, int toVersion);

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="upToVersion">Target aggregate version.</param>
        /// <returns>Domain events for the aggregat with the specified version.</returns>
        public virtual IDomainEventStream<TAggregateId> GetDomainEventStream(TAggregateId aggreggateId, int upToVersion)
        {
            return GetDomainEventStream(aggreggateId, 1, upToVersion);
        }

        /// <summary>
        /// Get all domain events of aggregate.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <returns>All domain events for the aggregate.</returns>
        public virtual IDomainEventStream<TAggregateId> GetDomainEventStream(TAggregateId aggreggateId)
        {
            return GetDomainEventStream(aggreggateId, int.MaxValue);
        }

        /// <summary>
        /// Commit the domain event to the store.
        /// </summary>
        /// <param name="domainEventStreamToCommit">Domain event to store.</param>
        protected abstract void Commit(IDomainEventStream<TAggregateId> domainEventStreamToCommit);

        /// <summary>
        /// Persist aggregate to the event store.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate to persist.</param>
        public void Save(TAggregate aggregateRoot)
        {
            try
            {
                // Get uncommited events.
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
