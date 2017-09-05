using System;

namespace Xer.Cqrs.Infrastructure.Repositories
{
    public interface IRepository<TAggregate> where TAggregate : AggregateRoot
    {
        void Save(TAggregate aggregate);
        TAggregate GetById(Guid aggregateId);
    }
}
