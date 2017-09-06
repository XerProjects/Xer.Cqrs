using System;
using System.Collections.Generic;

namespace Xer.Cqrs.EventSourcing.DomainEvents
{
    public interface IDomainEventStore<TAggregate> where TAggregate : EventSourcedAggregateRoot
    {
        void Save(TAggregate aggregateRoot);
        DomainEventStream GetDomainEventStream(Guid aggreggateId);
        IReadOnlyCollection<DomainEventStream> GetAllDomainEventStreams();
    }
}
