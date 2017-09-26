using System;
using System.Collections.Generic;
using System.Text;
using Xer.DomainDriven.EventSourcing.DomainEvents.Publishers;
using Xer.DomainDriven.EventSourcing.DomainEvents.Stores;
using Xer.DomainDriven.EventSourcing.DomainEvents.Subscriptions;
using Xer.DomainDriven.EventSourcing.Tests.Mocks;
using Xer.DomainDriven.EventSourcing.Tests.Mocks.DomainEvents;
using Xer.DomainDriven.EventSourcing.Tests.Mocks.Repositories;
using Xunit;

namespace Xer.DomainDriven.EventSourcing.Tests
{
    public class EventSourcedAggregateRepositoryTests
    {
        public class SaveMethod
        {
            [Fact]
            public void Save_Should_Append_To_Domain_Event_Store()
            {
                var subscription = new DomainEventSubscription();

                var publisher = new DomainEventPublisher(subscription);
                var eventStore = new InMemoryDomainEventStore<TestAggregate>(publisher);
                var repository = new TestEventSourcedAggregateRepository(eventStore);
                var id = Guid.NewGuid();
                var aggregate = new TestAggregate(id);
                repository.Save(aggregate);

                Assert.Equal(id, repository.GetById(id).Id);
            }
        }
    }
}
