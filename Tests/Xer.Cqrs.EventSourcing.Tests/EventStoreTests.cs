using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Events;
using Xer.Cqrs.EventSourcing.Repositories;
using Xer.Cqrs.EventSourcing.Tests.Mocks;
using Xer.Cqrs.EventSourcing.Tests.Mocks.DomainEventHandlers;
using Xer.Cqrs.EventSourcing.Tests.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.EventSourcing.Tests
{
    public class EventStoreTests
    {
        public class SaveMethod
        {
            [Fact]
            public void Should_Append_To_Domain_Event_Store()
            {
                IDomainEventStore<TestAggregate> eventStore = Factory.CreateEventStore<TestAggregate>();

                // Create aggregate.
                TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
                eventStore.Save(aggregate);

                DomainEventStream stream = eventStore.GetDomainEventStream(aggregate.Id);

                var fromStream = new TestAggregate(stream);

                Assert.NotNull(stream);
                Assert.Equal(aggregate.Id, fromStream.Id);
                
                // 1 domain event in total: Created event.
                Assert.Equal(1, stream.DomainEventCount);
            }
        }

        public class SaveAsyncMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public SaveAsyncMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public async Task Should_Append_To_Domain_Event_Store()
            {
                IDomainEventAsyncStore<TestAggregate> eventStore = Factory.CreateEventAsyncStore<TestAggregate>();

                // Create aggregate.
                TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
                await eventStore.SaveAsync(aggregate);

                DomainEventStream stream = await eventStore.GetDomainEventStreamAsync(aggregate.Id);

                Assert.NotNull(stream);
                Assert.Equal(aggregate.Id, stream.AggregateId);
                Assert.Equal(1, stream.DomainEventCount);
            }
        }

        public class GetDomainEventStreamMethod
        {
            [Fact]
            public void Should_Retrieve_Aggregate_Stream()
            {
                IDomainEventStore<TestAggregate> eventStore = Factory.CreateEventStore<TestAggregate>();

                // Create and modify aggregate.
                TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
                aggregate.ExecuteSomeOperation("I was modified!~");
                eventStore.Save(aggregate);

                DomainEventStream stream = eventStore.GetDomainEventStream(aggregate.Id);

                Assert.NotNull(stream);
                Assert.Equal(aggregate.Id, stream.AggregateId);

                // 2 domain events in total: Created + Modified events.
                Assert.Equal(2, stream.DomainEventCount);

                // Stream starts from version 1 to 2.
                Assert.Equal(1, stream.BeginVersion);
                Assert.Equal(2, stream.EndVersion);
            }
        }

        public class GetDomainEventStreamAsyncMethod
        {
            [Fact]
            public async Task GetDomainEventStreamAsync_Should_Retrieve_Aggregate_Stream()
            {
                IDomainEventAsyncStore<TestAggregate> eventStore = Factory.CreateEventAsyncStore<TestAggregate>();

                // Create and modify aggregate.
                TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
                aggregate.ExecuteSomeOperation("I was modified!~");
                await eventStore.SaveAsync(aggregate);

                DomainEventStream stream = await eventStore.GetDomainEventStreamAsync(aggregate.Id);

                Assert.NotNull(stream);
                Assert.Equal(aggregate.Id, stream.AggregateId);

                // 2 domain events in total: Created + Modified events.
                Assert.Equal(2, stream.DomainEventCount);

                // Stream starts from version 1 to 2.
                Assert.Equal(1, stream.BeginVersion);
                Assert.Equal(2, stream.EndVersion);
            }
        }
    }
}
