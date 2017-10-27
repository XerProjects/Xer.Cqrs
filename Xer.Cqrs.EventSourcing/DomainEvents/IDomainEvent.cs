using System;
using Xer.Cqrs.Events;

namespace Xer.Cqrs.EventSourcing.DomainEvents
{
    public interface IDomainEvent : IEvent
    {
        /// <summary>
        /// Id of the aggregate.
        /// </summary>
        Guid AggregateId { get; }

        /// <summary>
        /// Version of the aggregate.
        /// </summary>
        int AggregateVersion { get; }

        /// <summary>
        /// Time stamp.
        /// </summary>
        DateTime TimeStamp { get; }
    }
}
