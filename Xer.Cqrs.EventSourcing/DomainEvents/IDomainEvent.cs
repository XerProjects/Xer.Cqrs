using System;
using Xer.Cqrs.Events;

namespace Xer.Cqrs.EventSourcing.DomainEvents
{
    public interface IDomainEvent : IEvent
    {
        Guid AggregateId { get; }
        int AggregateVersion { get; }
        DateTime TimeStamp { get; }
    }
}
