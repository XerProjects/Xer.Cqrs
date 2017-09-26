using System;
using System.Collections.Generic;
using System.Text;
using Xer.DomainDriven.EventSourcing.DomainEvents;
using Xer.DomainDriven.EventSourcing.DomainEvents.Publishers;
using Xer.DomainDriven.EventSourcing.DomainEvents.Stores;
using Xer.DomainDriven.EventSourcing.DomainEvents.Subscriptions;
using Xer.DomainDriven.EventSourcing.Tests.Mocks;
using Xer.DomainDriven.EventSourcing.Tests.Mocks.DomainEventHandlers;
using Xer.DomainDriven.EventSourcing.Tests.Mocks.DomainEvents;
using Xer.DomainDriven.EventSourcing.Tests.Mocks.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace Xer.DomainDriven.EventSourcing.Tests
{
    public class DomainEventHandlerTests
    {
        public class HandleMethod
        {
            private readonly ITestOutputHelper _testOutput;

            public HandleMethod(ITestOutputHelper testOutput)
            {
                _testOutput = testOutput;
            }

            [Fact]
            public void Domain_Event_Handler_Should_Be_Invoked()
            {
                var handler = new TestDomainEventHandler(_testOutput);
                var subscription = new DomainEventSubscription();
                subscription.Subscribe((IDomainEventHandler<TestAggregateCreated>)handler);
                subscription.Subscribe((IDomainEventAsyncHandler<TestAggregateModified>)handler);
                var publisher = new DomainEventPublisher(subscription);
                var eventStore = new InMemoryDomainEventStore<TestAggregate>(publisher);
                var repository = new TestEventSourcedAggregateRepository(eventStore);
                var id = Guid.NewGuid();

                var aggregate = new TestAggregate(id);
                repository.Save(aggregate);

                Assert.Equal(id, repository.GetById(id).Id);

                Assert.Equal(1, handler.NumberOfTimesInvoked);

                aggregate.ChangeAggregateData("Test 1");
                repository.Save(aggregate);
                
                Assert.Equal(2, handler.NumberOfTimesInvoked);
            }
        }
    }
}
