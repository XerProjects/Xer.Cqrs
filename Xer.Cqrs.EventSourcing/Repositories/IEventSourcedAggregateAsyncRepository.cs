using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.DomainDriven.Repositories;

namespace Xer.Cqrs.EventSourcing.Repositories
{
    public interface IEventSourcedAggregateAsyncRepository<TAggregate> : IAggregateAsyncRepository<TAggregate> where TAggregate : IEventSourcedAggregate
    {
        Task<TAggregate> GetByIdAsync(Guid aggregateId, int version, CancellationToken cancellationToken = default(CancellationToken));
    }
}
