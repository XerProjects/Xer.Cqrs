using System;
using Xer.DomainDriven.Repositories;

namespace Xer.EventSourcing.Repositories
{
    public interface IEventSourcedAggregateRepository<TAggregate, TAggregateId> : IAggregateRepository<TAggregate, TAggregateId> 
                                                                                  where TAggregate : IEventSourcedAggregate<TAggregateId>
                                                                                  where TAggregateId : IEquatable<TAggregateId>
    {
        TAggregate GetById(TAggregateId aggregateId, int upToVersion);
        TAggregate GetById(TAggregateId aggregateId, int fromVersion, int toVersion);
    }
}
