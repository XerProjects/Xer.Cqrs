using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xer.DomainDriven.EventSourcing.DomainEvents;

namespace Xer.DomainDriven.EventSourcing.Repositories
{
    public abstract class EventSourcedAggregateAsyncRepository<TAggregate> : IEventSourcedAggregateAsyncRepository<TAggregate> where TAggregate : EventSourcedAggregate
    {
        protected virtual IDomainEventAsyncStore<TAggregate> DomainEventStore { get; }

        public EventSourcedAggregateAsyncRepository(IDomainEventAsyncStore<TAggregate> domainEventStore)
        {
            DomainEventStore = domainEventStore;
        }
        
        public abstract Task<TAggregate> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default(CancellationToken));
        public abstract Task<TAggregate> GetByIdAsync(Guid aggregateId, int version, CancellationToken cancellationToken = default(CancellationToken));
        public abstract Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken));
    }
}
