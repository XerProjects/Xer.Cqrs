using System;

namespace Xer.Cqrs.EventSourcing.DomainEvents
{
    public abstract class DomainEvent : IDomainEvent
    {
        public Guid AggregateId { get; private set; }
        public DateTime TimeStamp { get; private set; } = DateTime.Now;

        public DomainEvent(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }
}
