using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.DomainDriven.Repositories;

namespace Xer.Cqrs.EventSourcing.Repositories
{
    public interface IEventSourcedAggregateAsyncRepository<TAggregate, TAggregateId> : IAggregateAsyncRepository<TAggregate, TAggregateId> 
                                                                                       where TAggregate : IEventSourcedAggregate<TAggregateId>
                                                                                       where TAggregateId : IEquatable<TAggregateId>
    {
        Task<TAggregate> GetByIdAsync(TAggregateId aggregateId, int upToVersion, CancellationToken cancellationToken = default(CancellationToken));
        Task<TAggregate> GetByIdAsync(TAggregateId aggregateId, int fromVersion, int toVersion, CancellationToken cancellationToken = default(CancellationToken));
    }
}
