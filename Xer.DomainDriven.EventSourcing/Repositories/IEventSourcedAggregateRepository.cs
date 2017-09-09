using Xer.DomainDriven.Repositories;

namespace Xer.DomainDriven.EventSourcing.Repositories
{
    public interface IEventSourcedAggregateRepository<TAggregate> : IAggregateRepository<TAggregate> where TAggregate : EventSourcedAggregate
    {
        //void Save(TAggregate aggregate);
        //TAggregate GetById(Guid aggregateId);
    }
}
