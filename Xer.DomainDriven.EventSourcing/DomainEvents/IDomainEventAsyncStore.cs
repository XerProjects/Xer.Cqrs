using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.DomainDriven.EventSourcing.DomainEvents
{
    public interface IDomainEventAsyncStore<TAggregate> where TAggregate : EventSourcedAggregate
    {
        Task SaveAsync(TAggregate aggregateRoot, CancellationToken cancellationToken = default(CancellationToken));
        Task<DomainEventStream> GetDomainEventStreamAsync(Guid aggreggateId, CancellationToken cancellationToken = default(CancellationToken));
        Task<DomainEventStream> GetDomainEventStreamAsync(Guid aggreggateId, int version, CancellationToken cancellationToken = default(CancellationToken));
    }
}
