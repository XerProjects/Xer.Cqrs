using System;
using Xer.Cqrs.EventSourcing;
using Xer.DomainDriven.Repositories;

namespace Xer.Cqrs.EventSourcing.Repositories
{
    public interface IEventSourcedRepository<TAggregate> : IAggregateRepository<TAggregate> where TAggregate : EventSourcedAggregateRoot
    {
        //void Save(TAggregate aggregate);
        //TAggregate GetById(Guid aggregateId);
    }
}
