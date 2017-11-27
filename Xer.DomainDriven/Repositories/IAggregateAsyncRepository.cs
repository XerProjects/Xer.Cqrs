using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.DomainDriven.Repositories
{
    public interface IAggregateAsyncRepository<TAggregate, TAggregateId> where TAggregate : IAggregate<TAggregateId> 
                                                                         where TAggregateId : IEquatable<TAggregateId>
    {
        Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken));
        Task<TAggregate> GetByIdAsync(TAggregateId aggregateId, CancellationToken cancellationToken = default(CancellationToken));
    }
}
