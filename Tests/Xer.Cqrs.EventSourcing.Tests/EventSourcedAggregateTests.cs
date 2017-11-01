//using System;
//using Xer.Cqrs.EventSourcing.Tests.Mocks;
//using Xunit;

//namespace Xer.Cqrs.EventSourcing.Tests
//{
//    public class EventSourcedAggregateTests
//    {
//        #region Version Property

//        public class VersionProperty
//        {
//            [Fact]
//            public void Should_Be_Version_1_Initially()
//            {
//                var testAggregate = new TestAggregate(Guid.NewGuid());

//                Assert.Equal(1, testAggregate.Version);
//            }

//            [Fact]
//            public void Should_Increase_When_Event_Is_Applied()
//            {
//                var testAggregate = new TestAggregate(Guid.NewGuid());

//                Assert.Equal(1, testAggregate.Version);

//                // This should increment version by 1.
//                testAggregate.ExecuteSomeOperation("New Data");

//                Assert.Equal(2, testAggregate.Version);

//                // This should increment version by 1.
//                testAggregate.ExecuteSomeOperation("New Data2");

//                Assert.Equal(3, testAggregate.Version);
//            }
//        }

//        #endregion Version Property

//        #region ApplyChange Method

//        public class ApplyChangeMethod
//        {
//            [Fact]
//            public void Should_Update_Aggregate_When_Event_Is_Applied()
//            {
//                var testAggregate = new TestAggregate(Guid.NewGuid());

//                Assert.Equal(string.Empty, testAggregate.AggregateData);

//                // This should increment version by 1.
//                testAggregate.ExecuteSomeOperation("New Data");

//                Assert.Equal("New Data", testAggregate.AggregateData);

//                // This should increment version by 1.
//                testAggregate.ExecuteSomeOperation("New Data2");

//                Assert.Equal("New Data2", testAggregate.AggregateData);
//            }
//        }

//        #endregion ApplyChange Method

//        #region GetUncommittedEvents Method

//        public class GetUncommittedEventsMethod
//        {
//            [Fact]
//            public void Should_Return_Uncommitted_Events()
//            {
//                // Create aggregate with 1 uncommitted event.
//                var testAggregate = new TestAggregate(Guid.NewGuid());

//                // This should increment version by 4.
//                testAggregate.ExecuteSomeOperation("New Data1");
//                testAggregate.ExecuteSomeOperation("New Data2");
//                testAggregate.ExecuteSomeOperation("New Data3");
//                testAggregate.ExecuteSomeOperation("New Data4");

//                // Should have 5 uncommitted events. Created event + Update events.
//                Assert.Equal(5, testAggregate.GetUncommitedDomainEvents().DomainEventCount);
//            }
//        }

//        #endregion GetUncommittedEvents Method

//        #region ClearUncommittedEvents Method

//        public class ClearUncommittedEventsMethod
//        {
//            [Fact]
//            public void Should_Clear_All_Uncommitted_Events()
//            {
//                // Create aggregate with 1 uncommitted event.
//                var testAggregate = new TestAggregate(Guid.NewGuid());

//                // This should increment uncommitted events by 5.
//                testAggregate.ExecuteSomeOperation("New Data1");
//                testAggregate.ExecuteSomeOperation("New Data2");
//                testAggregate.ExecuteSomeOperation("New Data3");
//                testAggregate.ExecuteSomeOperation("New Data4");

//                // Should have 5 uncommitted events. Created event + Update events.
//                Assert.Equal(5, testAggregate.GetUncommitedDomainEvents().DomainEventCount);

//                testAggregate.ClearUncommitedDomainEvents();

//                // Should have 0 uncommitted events since it is cleared.
//                Assert.Equal(0, testAggregate.GetUncommitedDomainEvents().DomainEventCount);
//            }
//        }

//        #endregion ClearUncommittedEvents Method
//    }
//}
