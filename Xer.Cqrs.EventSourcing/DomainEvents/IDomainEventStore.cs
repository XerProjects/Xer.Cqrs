using System;

namespace Xer.Cqrs.EventSourcing.DomainEvents
{
    public interface IDomainEventStore<TAggregate> where TAggregate : EventSourcedAggregate
    {
        void Save(TAggregate aggregateRoot);
        DomainEventStream GetDomainEventStream(Guid aggreggateId);
        DomainEventStream GetDomainEventStream(Guid aggreggateId, int version);
    }
}
