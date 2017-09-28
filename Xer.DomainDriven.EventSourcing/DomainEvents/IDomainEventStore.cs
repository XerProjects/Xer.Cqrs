using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xer.DomainDriven.EventSourcing.DomainEvents
{
    public interface IDomainEventStore<TAggregate> where TAggregate : EventSourcedAggregate
    {
        void Save(TAggregate aggregateRoot);
        DomainEventStream GetDomainEventStream(Guid aggreggateId);
        DomainEventStream GetDomainEventStream(Guid aggreggateId, int version);
    }
}
