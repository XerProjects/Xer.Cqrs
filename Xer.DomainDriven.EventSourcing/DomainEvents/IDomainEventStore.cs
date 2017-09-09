using System;
using System.Collections.Generic;

namespace Xer.DomainDriven.EventSourcing.DomainEvents
{
    public interface IDomainEventStore<TAggregate> where TAggregate : EventSourcedAggregate
    {
        void Save(TAggregate aggregateRoot);
        DomainEventStream GetDomainEventStream(Guid aggreggateId);
        IReadOnlyCollection<DomainEventStream> GetAllDomainEventStreams();
    }
}
