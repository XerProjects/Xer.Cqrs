using System;

namespace Xer.EventSourcing
{    
    public interface IDomainEventStore<TAggregate, TAggregateId> where TAggregate : IEventSourcedAggregate<TAggregateId>
                                                                 where TAggregateId : IEquatable<TAggregateId>
    {
        /// <summary>
        /// Persist aggregate to the event store.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate to persist.</param>
        void Save(TAggregate aggregateRoot);

        /// <summary>
        /// Get all domain events of aggregate.
        /// </summary>
        /// <param name="aggregateId">ID of the aggregate.</param>
        /// <returns>All domain events for the aggregate.</returns>
        IDomainEventStream<TAggregateId> GetDomainEventStream(TAggregateId aggregateId);

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version.
        /// </summary>
        /// <param name="aggregateId">ID of the aggregate.</param>
        /// <param name="version">Target aggregate version.</param>
        /// <returns>Domain events for the aggregat with the specified version.</returns>
        IDomainEventStream<TAggregateId> GetDomainEventStream(TAggregateId aggregateId, int version);

        /// <summary>
        /// Get domain events of aggregate from the specified start and end version.
        /// </summary>
        /// <param name="aggregateId">ID of the aggregate.</param>
        /// <param name="fromVersion">Aggregate version to start retrieving domain events from.</param>
        /// <param name="toVersion">Target aggregate version.</param>
        /// <returns>Domain events for the aggregat with the specified version.</returns>
        IDomainEventStream<TAggregateId> GetDomainEventStream(TAggregateId aggregateId, int fromVersion, int toVersion);
    }
}
