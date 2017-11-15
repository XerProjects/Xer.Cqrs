using System;

namespace Xer.Cqrs.EventSourcing
{
    public interface IDomainEventStore<TAggregate> where TAggregate : IEventSourcedAggregate
    {
        /// <summary>
        /// Persist aggregate to the event store.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate to persist.</param>
        void Save(TAggregate aggregateRoot);

        /// <summary>
        /// Get all domain events of aggregate.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <returns>All domain events for the aggregate.</returns>
        DomainEventStream GetDomainEventStream(Guid aggreggateId);

        /// <summary>
        /// Get domain events of aggregate from the beginning up to the specified version.
        /// </summary>
        /// <param name="aggreggateId">ID of the aggregate.</param>
        /// <param name="version">Target aggregate version.</param>
        /// <returns>Domain events for the aggregat with the specified version.</returns>
        DomainEventStream GetDomainEventStream(Guid aggreggateId, int version);
    }
}
