using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xer.DomainDriven.Exceptions;

namespace Xer.DomainDriven.Repositories
{
    public class InMemoryAggregateRepository<TAggregate> : InMemoryAggregateRepository<TAggregate, Guid>
                                                           where TAggregate : IAggregate<Guid>
    {
        public InMemoryAggregateRepository()
        {
        }

        public InMemoryAggregateRepository(bool throwIfAggregateIsNotFound) : base(throwIfAggregateIsNotFound)
        {
        }
    }

    public class InMemoryAggregateRepository<TAggregate, TAggregateId> : IAggregateRepository<TAggregate, TAggregateId>, 
                                                                         IAggregateAsyncRepository<TAggregate, TAggregateId> 
                                                                         where TAggregate : IAggregate<TAggregateId>
                                                                         where TAggregateId : IEquatable<TAggregateId>
    {
        #region Declarations

        private static readonly Task CompletedTask = Task.FromResult(true);
        private List<TAggregate> _aggregates = new List<TAggregate>();
        
        private readonly bool _throwIfAggregateIsNotFound;

        #endregion Declarations

        #region Constructors

        public InMemoryAggregateRepository()
            : this(false)
        {

        }

        public InMemoryAggregateRepository(bool throwIfAggregateIsNotFound)
        {
            _throwIfAggregateIsNotFound = throwIfAggregateIsNotFound;
        }

        #endregion Constructors

        #region IAggregateRepository Implementation

        public TAggregate GetById(TAggregateId aggregateId)
        {
            TAggregate aggregate = _aggregates.FirstOrDefault(a => a.Id.Equals(aggregateId));
            if (aggregate == null && _throwIfAggregateIsNotFound)
            {
                throw new AggregateNotFoundException($"Aggregate of ID {aggregateId} was not found.");
            }

            return aggregate;
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

        public Task<TAggregate> GetByIdAsync(TAggregateId aggregateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                TAggregate aggregate = GetById(aggregateId);
                return Task.FromResult(aggregate);
            }
            catch (AggregateNotFoundException aex)
            {
                return TaskFromException<TAggregate>(aex);
            }
        }

        public Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                Save(aggregate);
                return CompletedTask;
            }
            catch (AggregateNotFoundException aex)
            {
                return TaskFromException<bool>(aex);
            }
        }

        #endregion IAggregateAsyncRepository Implementation

        #region Functions

        private static Task<TResult> TaskFromException<TResult>(Exception ex)
        {
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            tcs.TrySetException(ex);
            return tcs.Task;
        }

        #endregion Functions
    }
}
