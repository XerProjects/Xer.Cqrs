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
        /// Expected version of the aggregate after this event has been successfully applied.
        /// </summary>
        int AggregateVersion { get; }

        /// <summary>
        /// Timestamp.
        /// </summary>
        DateTime TimeStamp { get; }
    }
}
