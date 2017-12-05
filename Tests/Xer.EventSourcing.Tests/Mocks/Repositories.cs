using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.EventSourcing.Repositories;

namespace Xer.EventSourcing.Tests.Mocks
{
    #region TestAggregate Repositories

    public class TestAggregateRepository : EventSourcedAggregateRepository<TestAggregate, Guid>
    {
        protected override IDomainEventStore<TestAggregate, Guid> DomainEventStore { get; }

        public TestAggregateRepository(IDomainEventStore<TestAggregate, Guid> eventStore)
        {
            DomainEventStore = eventStore;
        }

        public override TestAggregate GetById(Guid aggregateId)
        {
            var history = DomainEventStore.GetDomainEventStream(aggregateId);

            return new TestAggregate(history);
        }

        public override TestAggregate GetById(Guid aggregateId, int version)
        {
            var history = DomainEventStore.GetDomainEventStream(aggregateId, version);

            return new TestAggregate(history);
        }

        public override void Save(TestAggregate aggregate)
        {
            DomainEventStore.Save(aggregate);
        }

        public override TestAggregate GetById(Guid aggregateId, int fromVersion, int toVersion)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Async repository.
    /// </summary>
    public class TestAggregateAsyncRepository : EventSourcedAggregateAsyncRepository<TestAggregate, Guid>
    {
        protected override IDomainEventAsyncStore<TestAggregate, Guid> DomainEventStore { get; }

        public TestAggregateAsyncRepository(IDomainEventAsyncStore<TestAggregate, Guid> domainEventStore)
        {
            DomainEventStore = domainEventStore;
        }

        public override async Task<TestAggregate> GetByIdAsync(Guid aggregateId, int fromVersion, int toVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            var history = await DomainEventStore.GetDomainEventStreamAsync(aggregateId, fromVersion, toVersion, cancellationToken);

            return new TestAggregate(history);
        }

        public override Task<TestAggregate> GetByIdAsync(Guid aggregateId, int upToVersion, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetByIdAsync(aggregateId, 1, upToVersion, cancellationToken);
        }

        public override Task<TestAggregate> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetByIdAsync(aggregateId, 1, int.MaxValue, cancellationToken);
        }

        public override Task SaveAsync(TestAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DomainEventStore.SaveAsync(aggregate, cancellationToken);
        }
    }

    #endregion TestAggregate Repositories
}
