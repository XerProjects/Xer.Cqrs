using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventSourcing.DomainEvents;

namespace Xer.Cqrs.EventSourcing.Repositories
{
    public abstract class EventSourcedAggregateAsyncRepository<TAggregate> : IEventSourcedAggregateAsyncRepository<TAggregate> where TAggregate : EventSourcedAggregate
    {
        protected IDomainEventAsyncStore<TAggregate> DomainEventStore { get; }

        public EventSourcedAggregateAsyncRepository(IDomainEventAsyncStore<TAggregate> domainEventStore)
        {
            DomainEventStore = domainEventStore;
        }
        
        public abstract Task<TAggregate> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default(CancellationToken));
        public abstract Task<TAggregate> GetByIdAsync(Guid aggregateId, int version, CancellationToken cancellationToken = default(CancellationToken));
        public abstract Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken));
    }
}
