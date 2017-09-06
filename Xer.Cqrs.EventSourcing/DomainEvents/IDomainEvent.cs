using System;

namespace Xer.Cqrs.EventSourcing.DomainEvents
{
    public interface IDomainEvent
    {
        Guid AggregateId { get; }
        int Version { get; }
        DateTime TimeStamp { get; }
    }
}
