using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.EventSourcing.Repositories
{
    public abstract class EventSourcedAggregateAsyncRepository<TAggregate> : IEventSourcedAggregateAsyncRepository<TAggregate> where TAggregate : IEventSourcedAggregate
    {
        protected abstract IDomainEventAsyncStore<TAggregate> DomainEventStore { get; }
        
        public abstract Task<TAggregate> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default(CancellationToken));
        public abstract Task<TAggregate> GetByIdAsync(Guid aggregateId, int version, CancellationToken cancellationToken = default(CancellationToken));

        public virtual Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DomainEventStore.SaveAsync(aggregate, cancellationToken);
        }
    }
}
