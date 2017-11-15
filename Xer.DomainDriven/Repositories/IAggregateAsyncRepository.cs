using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.DomainDriven.Repositories
{
    public interface IAggregateAsyncRepository<TAggregate> where TAggregate : IAggregate
    {
        Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken));
        Task<TAggregate> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default(CancellationToken));
    }
}
