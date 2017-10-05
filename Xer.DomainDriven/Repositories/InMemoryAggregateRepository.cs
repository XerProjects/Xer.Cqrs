using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xer.DomainDriven.Repositories
{
    public class InMemoryAggregateRepository<TAggregate> : IAggregateRepository<TAggregate>, IAggregateAsyncRepository<TAggregate> where TAggregate : Aggregate
    {
        #region Declarations

        private static readonly Task CompletedTask = Task.FromResult(0);
        private List<TAggregate> _aggregates = new List<TAggregate>();

        #endregion Declarations

        #region IAggregateRepository Implementation

        public TAggregate GetById(Guid aggregateId)
        {
            return _aggregates.FirstOrDefault(a => a.Id == aggregateId);
        }

        public void Save(TAggregate aggregate)
        {
            if (_aggregates.Contains(aggregate))
            {
                _aggregates.Remove(aggregate);
            }

            _aggregates.Add(aggregate);
        }

        #endregion IAggregateRepository Implementation

        #region IAggregateAsyncRepository Implementation

        public Task<TAggregate> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            TAggregate aggregate = GetById(aggregateId);

            return Task.FromResult(aggregate);
        }

        public Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            Save(aggregate);

            return CompletedTask;
        }

        #endregion IAggregateAsyncRepository Implementation
    }
}
