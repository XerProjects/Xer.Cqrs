using System;
using Xer.Cqrs.EventSourcing.DomainEvents;

namespace Xer.Cqrs.EventSourcing.Repositories
{
    public abstract class EventSourcedAggregateRepository<TAggregate> : IEventSourcedAggregateRepository<TAggregate> where TAggregate : EventSourcedAggregate
    {
        protected IDomainEventStore<TAggregate> DomainEventStore { get; }

        public EventSourcedAggregateRepository(IDomainEventStore<TAggregate> eventStore)
        {
            DomainEventStore = eventStore;
        }

        public abstract TAggregate GetById(Guid aggregateId);
        public abstract TAggregate GetById(Guid aggregateId, int version);
        public abstract void Save(TAggregate aggregate);
    }
}
