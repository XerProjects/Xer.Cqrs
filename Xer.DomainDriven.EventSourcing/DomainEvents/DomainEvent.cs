using System;

namespace Xer.DomainDriven.EventSourcing.DomainEvents
{
    public abstract class DomainEvent : IDomainEvent
    {
        public Guid AggregateId { get; }
        public int AggregateVersion { get; }
        public DateTime TimeStamp { get; } = DateTime.Now;

        /// <summary>
        /// Initializes an initial domain event (Version 1).
        /// </summary>
        /// <param name="aggregateId">Aggregate Id.</param>
        public DomainEvent(Guid aggregateId)
        {
            AggregateId = aggregateId;
            AggregateVersion = 1;
        }

        /// <summary>
        /// Initializes an initial domain event with the specified version.
        /// </summary>
        /// <param name="aggregateId">Aggregate Id.</param>
        /// <param name="aggregateVersion">Resulting version of the aggregate after applying this event.</param>
        public DomainEvent(Guid aggregateId, int aggregateVersion)
        {
            AggregateId = aggregateId;
            AggregateVersion = aggregateVersion;
        }

        /// <summary>
        /// Initializes a domain event with a bumped up version of the aggregate root.
        /// </summary>
        /// <param name="aggregateRoot">Aggregate root.</param>
        //public DomainEvent(EventSourcedAggregate aggregateRoot)
        //{
        //    if(aggregateRoot == null)
        //    {
        //        throw new ArgumentNullException(nameof(aggregateRoot));
        //    }

        //    AggregateId = aggregateRoot.Id;
        //    AggregateVersion = aggregateRoot.Version + 1; // Bump up current version for event.
        //}
    }
}
