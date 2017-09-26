using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xer.DomainDriven.EventSourcing.DomainEvents;
using Xer.DomainDriven.EventSourcing.Repositories;

namespace Xer.DomainDriven.EventSourcing.Tests.Mocks.Repositories
{
    /// <summary>
    /// Sync repository.
    /// </summary>
    public class TestEventSourcedAggregateRepository : EventSourcedAggregateRepository<TestAggregate>
    {
        public TestEventSourcedAggregateRepository(IDomainEventStore<TestAggregate> eventStore) 
            : base(eventStore)
        {
        }

        public override TestAggregate GetById(Guid aggregateId)
        {
            var history = DomainEventStore.GetDomainEventStream(aggregateId);

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
    public class TestEventSourcedAggregateAsyncRepository : EventSourcedAggregateAsyncRepository<TestAggregate>
    {
        public TestEventSourcedAggregateAsyncRepository(IDomainEventAsyncStore<TestAggregate> eventStore)
            : base(eventStore)
        {
        }

        public override async Task<TestAggregate> GetByIdAsync(Guid aggregateId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var history = await DomainEventStore.GetDomainEventStreamAsync(aggregateId);

            return new TestAggregate(history);
        }

        public override Task SaveAsync(TestAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DomainEventStore.SaveAsync(aggregate);
        }
    }
}
