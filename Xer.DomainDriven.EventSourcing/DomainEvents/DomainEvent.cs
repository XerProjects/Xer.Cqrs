using System;

namespace Xer.DomainDriven.EventSourcing.DomainEvents
{
    public abstract class DomainEvent : IDomainEvent
    {
        public Guid AggregateId { get; }
        public int Version { get; }
        public DateTime TimeStamp { get; } = DateTime.Now;

        /// <summary>
        /// Initializes a domain event with a bumped up version of the aggregate root.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate root.</param>
        public DomainEvent(EventSourcedAggregate aggregateRoot)
        {
            if(aggregateRoot == null)
            {
                throw new ArgumentNullException(nameof(aggregateRoot));
            }

            AggregateId = aggregateRoot.Id;
            Version = aggregateRoot.Version + 1; // Bump up current version for event.
        }

        /// <summary>
        /// Initializes an initial domain event (Version 0).
        /// </summary>
        /// <param name="aggregateId">Aggregate Id.</param>
        public DomainEvent(Guid aggregateId)
        {
            AggregateId = aggregateId;
            Version = 0;
        }
    }
}
