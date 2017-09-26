using System;
using System.Collections.Generic;
using System.Text;
using Xer.DomainDriven.EventSourcing.DomainEvents;

namespace Xer.DomainDriven.EventSourcing.Repositories
{
    public abstract class EventSourcedAggregateRepository<TAggregate> : IEventSourcedAggregateRepository<TAggregate> where TAggregate : EventSourcedAggregate
    {
        protected virtual IDomainEventStore<TAggregate> DomainEventStore { get; }

        public EventSourcedAggregateRepository(IDomainEventStore<TAggregate> eventStore)
        {
            DomainEventStore = eventStore;
        }

        public abstract TAggregate GetById(Guid aggregateId);
        public abstract void Save(TAggregate aggregate);
    }
}
