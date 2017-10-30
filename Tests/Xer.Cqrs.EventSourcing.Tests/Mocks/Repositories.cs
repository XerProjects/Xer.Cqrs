using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventSourcing.DomainEvents;
using Xer.Cqrs.EventSourcing.Repositories;

namespace Xer.Cqrs.EventSourcing.Tests.Mocks
{
    #region TestAggregate Repositories

    public class TestAggregateRepository : EventSourcedAggregateRepository<TestAggregate>
    {
        protected override IDomainEventStore<TestAggregate> DomainEventStore { get; }

        public TestAggregateRepository(IDomainEventStore<TestAggregate> eventStore)
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
    }

    /// <summary>
    /// Async repository.
    /// </summary>
    public class TestAggregateAsyncRepository : EventSourcedAggregateAsyncRepository<TestAggregate>
    {
        protected override IDomainEventAsyncStore<TestAggregate> DomainEventStore { get; }

        public TestAggregateAsyncRepository(IDomainEventAsyncStore<TestAggregate> domainEventStore)
        {
            DomainEventStore = domainEventStore;
        }

        public override async Task<TestAggregate> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var history = await DomainEventStore.GetDomainEventStreamAsync(aggregateId, cancellationToken);

            return new TestAggregate(history);
        }

        public override async Task<TestAggregate> GetByIdAsync(Guid aggregateId, int version, CancellationToken cancellationToken = default(CancellationToken))
        {
            var history = await DomainEventStore.GetDomainEventStreamAsync(aggregateId, version, cancellationToken);

            return new TestAggregate(history);
        }

        public override Task SaveAsync(TestAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DomainEventStore.SaveAsync(aggregate);
        }
    }

    #endregion TestAggregate Repositories
}
