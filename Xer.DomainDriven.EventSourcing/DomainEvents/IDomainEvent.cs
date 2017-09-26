using System;

namespace Xer.DomainDriven.EventSourcing.DomainEvents
{
    public interface IDomainEvent
    {
        Guid AggregateId { get; }
        int AggregateVersion { get; }
        DateTime TimeStamp { get; }
    }
}
