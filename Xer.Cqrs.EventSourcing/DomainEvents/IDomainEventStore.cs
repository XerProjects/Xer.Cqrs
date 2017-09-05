using System;
using System.Collections.Generic;
using System.Linq;

namespace Xer.Cqrs.EventSourcing.DomainEvents
{
    public interface IDomainEventStore<TAggregate> where TAggregate : EventSourcedAggregateRoot
    {
        void Save(TAggregate aggregateRoot);
        IReadOnlyCollection<IDomainEvent> GetDomainEventStream(Guid aggreggateId);
        ILookup<Guid, IReadOnlyCollection<IDomainEvent>> GetAllDomainEventStreamsGroupedById();
    }
}
