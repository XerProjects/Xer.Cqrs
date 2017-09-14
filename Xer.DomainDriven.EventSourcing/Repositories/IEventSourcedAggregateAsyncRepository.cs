using Xer.DomainDriven.Repositories;

namespace Xer.DomainDriven.EventSourcing.Repositories
{
    public interface IEventSourcedAggregateAsyncRepository<TAggregate> : IAggregateAsyncRepository<TAggregate> where TAggregate : EventSourcedAggregate
    {
    }
}
