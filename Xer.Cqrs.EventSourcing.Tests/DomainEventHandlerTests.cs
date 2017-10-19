using System;
using System.Threading;
using Xer.Cqrs.Events.Publishers;
using Xer.Cqrs.Events.Registrations;
using Xer.Cqrs.EventSourcing.DomainEvents;
using Xer.Cqrs.EventSourcing.DomainEvents.Stores;
using Xer.Cqrs.EventSourcing.Tests.Mocks;
using Xer.Cqrs.EventSourcing.Tests.Mocks.DomainEventHandlers;
using Xer.Cqrs.EventSourcing.Tests.Mocks.DomainEvents;
using Xer.Cqrs.EventSourcing.Tests.Mocks.Repositories;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.EventSourcing.Tests
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

                var subscription = new EventHandlerFactoryRegistration();
                subscription.Register<TestAggregateCreated>(() => handler);
                subscription.Register<TestAggregateModified>(() => handler);

                var publisher = new EventPublisher(subscription);
                var eventStore = new InMemoryDomainEventStore<TestAggregate>(publisher);
                var repository = new TestEventSourcedAggregateRepository(eventStore);
                var id = Guid.NewGuid();

                var aggregate = new TestAggregate(id);
                repository.Save(aggregate);

                // Event may not have yet been handled in background.
                Thread.Sleep(500);

                Assert.Equal(id, repository.GetById(id).Id);

                Assert.Equal(1, handler.NumberOfTimesInvoked);

                aggregate.ChangeAggregateData("Test 1");
                repository.Save(aggregate);

                // Event may not have yet been handled in background.
                Thread.Sleep(500);
                
                Assert.Equal(2, handler.NumberOfTimesInvoked);
            }

            [Fact]
            public void Domain_Event_Handler_Exception_Should_Be_Ignored()
            {
                var handler = new TestDomainEventHandler(_testOutput);

                var subscription = new EventHandlerFactoryRegistration();
                subscription.Register<TestAggregateCreated>(() => handler);
                subscription.Register<TestAggregateModified>(() => handler);

                var publisher = new EventPublisher(subscription);
                var eventStore = new InMemoryDomainEventStore<TestAggregate>(publisher);
                var repository = new TestEventSourcedAggregateRepository(eventStore);
                var id = Guid.NewGuid();

                var aggregate = new TestAggregate(id);
                repository.Save(aggregate);

                // Event may not have yet been handled in background.
                Thread.Sleep(1000);

                Assert.Equal(id, repository.GetById(id).Id);

                Assert.Equal(1, handler.NumberOfTimesInvoked);

                Assert.ThrowsAny<Exception>(() =>
                {
                    aggregate.ThrowExceptionOnEventHandler();
                    repository.Save(aggregate);
                });
            }
        }
    }
}
