using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.Cqrs.EventSourcing.Repositories
{
    public abstract class EventSourcedAggregateAsyncRepository<TAggregate, TAggregateId> : IEventSourcedAggregateAsyncRepository<TAggregate, TAggregateId> 
                                                                                           where TAggregate : IEventSourcedAggregate<TAggregateId>
                                                                                           where TAggregateId : IEquatable<TAggregateId>
    {
        protected abstract IDomainEventAsyncStore<TAggregate, TAggregateId> DomainEventStore { get; }
        
        public abstract Task<TAggregate> GetByIdAsync(TAggregateId aggregateId, int fromVersion, int toVersion, CancellationToken cancellationToken = default(CancellationToken));
        
        public virtual Task<TAggregate> GetByIdAsync(TAggregateId aggregateId, int upToVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetByIdAsync(aggregateId, 1, upToVersion, cancellationToken);
        }
        public virtual Task<TAggregate> GetByIdAsync(TAggregateId aggregateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetByIdAsync(aggregateId, 1, int.MaxValue, cancellationToken);
        }
        
        public virtual Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DomainEventStore.SaveAsync(aggregate, cancellationToken);
        }
    }
}
