using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack;
using Xer.EventSourcing.Repositories;
using Xer.EventSourcing.Tests.Mocks;
using Xer.EventSourcing.Tests.Mocks.DomainEventHandlers;
using Xer.EventSourcing.Tests.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Xer.EventSourcing.Tests
{
    public class EventStoreTests
    {
        public class SaveMethod
        {
            [Fact]
            public void Should_Append_To_Domain_Event_Store()
            {
                IDomainEventStore<TestAggregate, Guid> eventStore = Factory.CreateEventStore<TestAggregate, Guid>();

                // Create aggregate.
                TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
                eventStore.Save(aggregate);

                IDomainEventStream<Guid> stream = eventStore.GetDomainEventStream(aggregate.Id);

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
                IDomainEventAsyncStore<TestAggregate, Guid> eventStore = Factory.CreateEventAsyncStore<TestAggregate, Guid>();

                // Create aggregate.
                TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
                await eventStore.SaveAsync(aggregate);

                IDomainEventStream<Guid> stream = await eventStore.GetDomainEventStreamAsync(aggregate.Id);

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
                IDomainEventStore<TestAggregate, Guid> eventStore = Factory.CreateEventStore<TestAggregate, Guid>();

                // Create and modify aggregate.
                TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
                aggregate.ExecuteSomeOperation("I was modified!~");
                eventStore.Save(aggregate);

                IDomainEventStream<Guid> stream = eventStore.GetDomainEventStream(aggregate.Id);

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
                IDomainEventAsyncStore<TestAggregate, Guid> eventStore = Factory.CreateEventAsyncStore<TestAggregate, Guid>();

                // Create and modify aggregate.
                TestAggregate aggregate = new TestAggregate(Guid.NewGuid());
                aggregate.ExecuteSomeOperation("I was modified!~");
                await eventStore.SaveAsync(aggregate);

                IDomainEventStream<Guid> stream = await eventStore.GetDomainEventStreamAsync(aggregate.Id);

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
