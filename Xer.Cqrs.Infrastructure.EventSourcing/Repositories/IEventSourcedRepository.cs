using Xer.Cqrs.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xer.Cqrs.Infrastructure.EventSourcing.Repositories
{
    public interface IEventSourcedRepository<TAggregate> where TAggregate : EventSourcedAggregateRoot
    {
        void Save(TAggregate aggregate);
        TAggregate GetById(Guid aggregateId);
    }
}
