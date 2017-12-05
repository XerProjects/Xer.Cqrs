using System;
using System.Threading;
using Xer.Cqrs.EventStack;
using Xer.EventSourcing.Repositories;
using Xer.EventSourcing.Tests.Mocks;
using Xer.EventSourcing.Tests.Mocks.DomainEventHandlers;
using Xer.EventSourcing.Tests.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Xer.EventSourcing.Tests
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

                IEventSourcedAggregateRepository<TestAggregate, Guid> repository = Factory.CreateTestAggregateRepository(reg =>
                {
                    reg.Register<TestAggregateCreatedEvent>(() => handler);
                    reg.Register<OperationExecutedEvent>(() => handler);
                });

                TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
                aggregate.ExecuteSomeOperation("Test Operation");
                repository.Save(aggregate);

                // Event may not have yet been handled in background.
                Thread.Sleep(1000);

                // Aggregate should be stored.
                TestAggregate storedAggregate = repository.GetById(aggregate.Id);
                Assert.Equal(aggregate.Id, storedAggregate.Id);

                // Aggregate should have 2 events.
                // 1. TestAggregateCreated
                // 2. TestAggregateModified
                Assert.Equal(2, handler.HandledEvents.Count);
                Assert.Contains(handler.HandledEvents, (e) => e is TestAggregateCreatedEvent);
                Assert.Contains(handler.HandledEvents, (e) => e is OperationExecutedEvent);
            }

            [Fact]
            public void Should_Invoke_Multiple_Registered_Domain_Event_Handlers()
            {
                TestDomainEventHandler handler1 = new TestDomainEventHandler(_testOutput);
                TestDomainEventHandler handler2 = new TestDomainEventHandler(_testOutput);
                TestDomainEventHandler handler3 = new TestDomainEventHandler(_testOutput);

                IEventSourcedAggregateRepository<TestAggregate, Guid> repository = Factory.CreateTestAggregateRepository(reg =>
                {
                    reg.Register<TestAggregateCreatedEvent>(() => handler1);
                    reg.Register<OperationExecutedEvent>(() => handler1);
                    reg.Register<TestAggregateCreatedEvent>(() => handler2);
                    reg.Register<OperationExecutedEvent>(() => handler2);
                    reg.Register<TestAggregateCreatedEvent>(() => handler3);
                    reg.Register<OperationExecutedEvent>(() => handler3);
                });

                TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
                aggregate.ExecuteSomeOperation("Test Operation");
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
                Assert.Contains(handler1.HandledEvents, (e) => e is TestAggregateCreatedEvent);
                Assert.Contains(handler1.HandledEvents, (e) => e is OperationExecutedEvent);

                // Handler2 should have 2 events.
                // 1. TestAggregateCreated
                // 2. TestAggregateModified
                Assert.Equal(2, handler2.HandledEvents.Count);
                Assert.Contains(handler2.HandledEvents, (e) => e is TestAggregateCreatedEvent);
                Assert.Contains(handler2.HandledEvents, (e) => e is OperationExecutedEvent);

                // Handler3 should have 2 events.
                // 1. TestAggregateCreated
                // 2. TestAggregateModified
                Assert.Equal(2, handler3.HandledEvents.Count);
                Assert.Contains(handler3.HandledEvents, (e) => e is TestAggregateCreatedEvent);
                Assert.Contains(handler3.HandledEvents, (e) => e is OperationExecutedEvent);
            }

            [Fact]
            public void Should_Trigger_OnError_When_Domain_Event_Handler_Exception_Occurred()
            {
                TestDomainEventHandler handler = new TestDomainEventHandler(_testOutput);

                IEventPublisher publisher = Factory.CreatePublisher(reg =>
                {
                    reg.Register<TestAggregateCreatedEvent>(() => handler);
                    reg.Register<OperationExecutedEvent>(() => handler);
                });

                publisher.OnError += (e, ex) =>
                {
                    _testOutput.WriteLine($"{ex.GetType().Name} occurred while handling {e.GetType().Name} event: {ex.Message}");

                    Assert.IsType<TestAggregateDomainEventHandlerException>(ex);
                };

                IEventSourcedAggregateRepository<TestAggregate, Guid> repository = Factory.CreateTestAggregateRepository(publisher);
                                
                TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
                // This would trigger a TestAggregateDomainEventHandlerException when handled by TestDomainEventHandler.
                aggregate.TriggerExceptionOnEventHandler();
                repository.Save(aggregate);
            }
        }
    }
}
