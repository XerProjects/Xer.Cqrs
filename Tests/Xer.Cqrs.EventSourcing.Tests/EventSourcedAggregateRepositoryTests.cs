using System;
using Xer.Cqrs.Events.Publishers;
using Xer.Cqrs.Events.Registrations;
using Xer.Cqrs.EventSourcing.DomainEvents.Stores;
using Xer.Cqrs.EventSourcing.Tests.Mocks;
using Xer.Cqrs.EventSourcing.Tests.Mocks.Repositories;
using Xunit;

namespace Xer.Cqrs.EventSourcing.Tests
{
    public class EventSourcedAggregateRepositoryTests
    {
        public class SaveMethod
        {
            [Fact]
            public void Save_Should_Append_To_Domain_Event_Store()
            {
                var subscription = new EventHandlerRegistration();

                var publisher = new EventPublisher(subscription);
                var eventStore = new PublishingDomainEventStore<TestAggregate>(new InMemoryDomainEventStore<TestAggregate>(), publisher);
                var repository = new TestEventSourcedAggregateRepository(eventStore);
                var id = Guid.NewGuid();
                var aggregate = new TestAggregate(id);
                repository.Save(aggregate);

                Assert.Equal(id, repository.GetById(id).Id);
            }
        }
    }
}
