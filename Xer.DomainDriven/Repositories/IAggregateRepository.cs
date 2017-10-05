using System;

namespace Xer.DomainDriven.Repositories
{
    public interface IAggregateRepository<TAggregate> where TAggregate : Aggregate
    {
        void Save(TAggregate aggregate);
        TAggregate GetById(Guid aggregateId);
    }
}
