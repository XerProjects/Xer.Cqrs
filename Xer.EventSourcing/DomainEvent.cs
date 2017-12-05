using System;

namespace Xer.EventSourcing
{
    public abstract class DomainEvent : IDomainEvent
    {
        /// <summary>
        /// Id of the aggregate.
        /// </summary>
        public Guid AggregateId { get; }

        /// <summary>
        /// Expected version of the aggregate after this event has been successfully applied.
        /// </summary>
        public int AggregateVersion { get; }

        /// <summary>
        /// Timestamp. By default, this is set to current local time.
        /// </summary>
        public virtual DateTime TimeStamp { get; } = DateTime.Now;

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
        /// Initializes a domain event with the specified expected version.
        /// </summary>
        /// <param name="aggregateId">Aggregate Id.</param>
        /// <param name="nextExpectedAggregateVersion">Next expected aggregate version after this event has been applied.</param>
        public DomainEvent(Guid aggregateId, int nextExpectedAggregateVersion)
        {
            AggregateId = aggregateId;
            AggregateVersion = nextExpectedAggregateVersion;
        }
    }
}
