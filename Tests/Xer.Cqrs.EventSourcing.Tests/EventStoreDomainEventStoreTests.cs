//using System;
//using System.Threading.Tasks;
//using Xer.DomainDriven.EventSourcing.DomainEvents;
//using Xer.DomainDriven.EventSourcing.DomainEvents.Publishers;
//using Xer.DomainDriven.EventSourcing.DomainEvents.Subscriptions;
//using Xer.DomainDriven.EventSourcing.EventStore;
//using Xer.DomainDriven.EventSourcing.Tests.Mocks;
//using Xunit;

//namespace Xer.DomainDriven.EventSourcing.Tests
//{
//    public class EventStoreDomainEventStoreTests
//    {
//        public class SaveAsyncMethod
//        {
//            [Fact]
//            public async Task Should_Save_Aggregate_In_EventStore()
//            {
//                var registration = new DomainEventSubscription();
//                var eventStore = new EventStoreDomainEventStore<TestAggregate>(new DomainEventPublisher(registration), new EventStoreConfiguration("admin", "changeit", System.Net.IPAddress.Loopback.ToString(), 1113, 4096));

//                Guid newGuid = Guid.NewGuid();

//                var testAggregate = new TestAggregate(newGuid);

//                await eventStore.SaveAsync(testAggregate);

//                DomainEventStream stream = await eventStore.GetDomainEventStreamAsync(newGuid);

//                Assert.Equal(newGuid, stream.AggregateId);
//            }

//            [Fact]
//            public async Task Should_Update_Aggregate_Version_In_EventStore()
//            {
//                var registration = new DomainEventSubscription();
//                var eventStore = new EventStoreDomainEventStore<TestAggregate>(new DomainEventPublisher(registration), new EventStoreConfiguration("admin", "changeit", System.Net.IPAddress.Loopback.ToString(), 1113, 4096));

//                Guid newGuid = Guid.NewGuid();

//                var testAggregate = new TestAggregate(newGuid);
//                testAggregate.ChangeAggregateData("Test 1");
//                testAggregate.ChangeAggregateData("Test 2");
//                testAggregate.ChangeAggregateData("Test 3");
//                testAggregate.ChangeAggregateData("Test 4");

//                await eventStore.SaveAsync(testAggregate);

//                DomainEventStream stream = await eventStore.GetDomainEventStreamAsync(newGuid);

//                var testAggregateCopy = new TestAggregate(stream);

//                Assert.Equal(newGuid, testAggregateCopy.Id);
//                Assert.Equal(testAggregate.AggregateData, testAggregateCopy.AggregateData);
//            }
//        }
//    }
//}
