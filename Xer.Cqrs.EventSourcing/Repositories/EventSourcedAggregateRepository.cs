using System;

namespace Xer.Cqrs.EventSourcing.Repositories
{
    public abstract class EventSourcedAggregateRepository<TAggregate, TAggregateId> : IEventSourcedAggregateRepository<TAggregate, TAggregateId> 
                                                                                      where TAggregate : IEventSourcedAggregate<TAggregateId>
                                                                                      where TAggregateId : IEquatable<TAggregateId>
    {
        protected abstract IDomainEventStore<TAggregate, TAggregateId> DomainEventStore { get; }

        public abstract TAggregate GetById(TAggregateId aggregateId, int fromVersion, int toVersion);

        public virtual TAggregate GetById(TAggregateId aggregateId, int upToVersion)
        {
            return GetById(aggregateId, 1, upToVersion);
        }
        public virtual TAggregate GetById(TAggregateId aggregateId)
        {
            return GetById(aggregateId, 1, int.MaxValue);
        }

        public virtual void Save(TAggregate aggregate)
        {
            DomainEventStore.Save(aggregate);
        }
    }
}
