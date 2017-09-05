using System;

namespace Xer.Cqrs.EventSourcing.DomainEvents
{
    public interface IDomainEvent
    {
        Guid AggregateId { get; }
        DateTime TimeStamp { get; }
    }
}
