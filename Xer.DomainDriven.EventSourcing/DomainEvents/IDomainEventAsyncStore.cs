using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xer.DomainDriven.EventSourcing.DomainEvents
{
    public interface IDomainEventAsyncStore<TAggregate> where TAggregate : EventSourcedAggregate
    {
        Task SaveAsync(TAggregate aggregateRoot);
        Task<DomainEventStream> GetDomainEventStreamAsync(Guid aggreggateId);
        Task<DomainEventStream> GetDomainEventStreamAsync(Guid aggreggateId, int version = 1);
    }
}
