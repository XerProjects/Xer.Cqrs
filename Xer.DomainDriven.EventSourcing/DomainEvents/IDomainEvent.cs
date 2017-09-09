using System;

namespace Xer.DomainDriven.EventSourcing.DomainEvents
{
    public interface IDomainEvent
    {
        Guid AggregateId { get; }
        int Version { get; }
        DateTime TimeStamp { get; }
    }
}
