using System;
using System.Threading;
using Xer.Cqrs.Events;
using Xer.Cqrs.EventSourcing.Repositories;
using Xer.Cqrs.EventSourcing.Tests.Mocks;
using Xer.Cqrs.EventSourcing.Tests.Mocks.DomainEventHandlers;
using Xer.Cqrs.EventSourcing.Tests.Utilities;
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
            public void Should_Invoke_Registered_Domain_Event_Handler()
            {
                TestDomainEventHandler handler = new TestDomainEventHandler(_testOutput);

                IEventSourcedAggregateRepository<TestAggregate> repository = Factory.CreateTestAggregateRepository(reg =>
                {
                    reg.Register<TestAggregateCreated>(() => handler);
                    reg.Register<TestAggregateModified>(() => handler);
                });

                TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
                aggregate.ChangeAggregateData("Test 1");
                repository.Save(aggregate);

                // Event may not have yet been handled in background.
                Thread.Sleep(500);

                // Aggregate should be stored.
                TestAggregate storedAggregate = repository.GetById(aggregate.Id);
                Assert.Equal(aggregate.Id, storedAggregate.Id);

                // Aggregate should have 2 events.
                // 1. TestAggregateCreated
                // 2. TestAggregateModified
                Assert.Equal(2, handler.HandledEvents.Count);
                Assert.Contains(handler.HandledEvents, (e) => e is TestAggregateCreated);
                Assert.Contains(handler.HandledEvents, (e) => e is TestAggregateModified);
            }

            [Fact]
            public void Should_Invoke_Multiple_Registered_Domain_Event_Handlers()
            {
                TestDomainEventHandler handler1 = new TestDomainEventHandler(_testOutput);
                TestDomainEventHandler handler2 = new TestDomainEventHandler(_testOutput);
                TestDomainEventHandler handler3 = new TestDomainEventHandler(_testOutput);

                IEventSourcedAggregateRepository<TestAggregate> repository = Factory.CreateTestAggregateRepository(reg =>
                {
                    reg.Register<TestAggregateCreated>(() => handler1);
                    reg.Register<TestAggregateModified>(() => handler1);
                    reg.Register<TestAggregateCreated>(() => handler2);
                    reg.Register<TestAggregateModified>(() => handler2);
                    reg.Register<TestAggregateCreated>(() => handler3);
                    reg.Register<TestAggregateModified>(() => handler3);
                });

                TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
                aggregate.ChangeAggregateData("Test 1");
                repository.Save(aggregate);

                // Event may not have yet been handled in background.
                Thread.Sleep(500);

                // Aggregate should be stored.
                TestAggregate storedAggregate = repository.GetById(aggregate.Id);
                Assert.Equal(aggregate.Id, storedAggregate.Id);

                // Handler1 should have 2 events.
                // 1. TestAggregateCreated
                // 2. TestAggregateModified
                Assert.Equal(2, handler1.HandledEvents.Count);
                Assert.Contains(handler1.HandledEvents, (e) => e is TestAggregateCreated);
                Assert.Contains(handler1.HandledEvents, (e) => e is TestAggregateModified);

                // Handler2 should have 2 events.
                // 1. TestAggregateCreated
                // 2. TestAggregateModified
                Assert.Equal(2, handler2.HandledEvents.Count);
                Assert.Contains(handler2.HandledEvents, (e) => e is TestAggregateCreated);
                Assert.Contains(handler2.HandledEvents, (e) => e is TestAggregateModified);

                // Handler3 should have 2 events.
                // 1. TestAggregateCreated
                // 2. TestAggregateModified
                Assert.Equal(2, handler3.HandledEvents.Count);
                Assert.Contains(handler3.HandledEvents, (e) => e is TestAggregateCreated);
                Assert.Contains(handler3.HandledEvents, (e) => e is TestAggregateModified);
            }

            [Fact]
            public void Should_Trigger_OnError_When_Domain_Event_Handler_Exception_Occurred()
            {
                TestDomainEventHandler handler = new TestDomainEventHandler(_testOutput);

                IEventPublisher publisher = Factory.CreatePublisher(reg =>
                {
                    reg.Register<TestAggregateCreated>(() => handler);
                    reg.Register<TestAggregateModified>(() => handler);
                });

                publisher.OnError += (e, ex) =>
                {
                    _testOutput.WriteLine($"Handled {ex.GetType().Name}.");

                    Assert.IsType<TestDomainEventHandlerException>(ex);
                };

                IEventSourcedAggregateRepository<TestAggregate> repository = Factory.CreateTestAggregateRepository(publisher);
                                
                TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
                // This would trigger a TestDomainEventHandlerException when handled by TestDomainEventHandler.
                aggregate.ThrowExceptionOnEventHandler();
                repository.Save(aggregate);
            }
        }
    }
}
