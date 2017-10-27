using System;
using Xer.Cqrs.EventSourcing.Tests.Mocks;
using Xunit;

namespace Xer.Cqrs.EventSourcing.Tests
{
    public class EventSourcedAggregateTests
    {
        public class VersionProperty
        {
            [Fact]
            public void Initial_Aggregate_Version_Must_Be_1()
            {
                var testAggregate = new TestAggregate(Guid.NewGuid());

                Assert.Equal(testAggregate.Version, 1);
            }

            [Fact]
            public void Aggregate_Version_Must_Increase_When_Event_Is_Applied()
            {
                var testAggregate = new TestAggregate(Guid.NewGuid());
                
                Assert.Equal(1, testAggregate.Version);

                // This should increment version by 1.
                testAggregate.ChangeAggregateData("New Data");

                Assert.Equal(2, testAggregate.Version);

                // This should increment version by 1.
                testAggregate.ChangeAggregateData("New Data2");

                Assert.Equal(3, testAggregate.Version);
            }
        }

        public class ApplyChangeMethod
        {
            [Fact]
            public void Aggregate_Data_Must_Update_When_Event_Is_Applied()
            {
                var testAggregate = new TestAggregate(Guid.NewGuid());

                Assert.Equal(string.Empty, testAggregate.AggregateData);

                // This should increment version by 1.
                testAggregate.ChangeAggregateData("New Data");

                Assert.Equal("New Data", testAggregate.AggregateData);

                // This should increment version by 1.
                testAggregate.ChangeAggregateData("New Data2");

                Assert.Equal("New Data2", testAggregate.AggregateData);
            }
        }

        //public class GetUncommittedEventsMethod
        //{
        //    [Fact]
        //    public void Aggregate_Should_Have_Uncommitted_Events()
        //    {
        //        var testAggregate = new TestAggregate(Guid.NewGuid());

        //        // This should increment version by 1.
        //        testAggregate.ChangeAggregateData("New Data1");
        //        // This should increment version by 1.
        //        testAggregate.ChangeAggregateData("New Data2");
        //        // This should increment version by 1.
        //        testAggregate.ChangeAggregateData("New Data3");
        //        // This should increment version by 1.
        //        testAggregate.ChangeAggregateData("New Data4");

        //        // Should have 5 uncommitted events. Created event + Modified events.
        //        Assert.Equal(5, testAggregate.GetUncommitedDomainEvents().DomainEventCount);
        //    }

        //    [Fact]
        //    public void Aggregate_Should_Have_Uncommitted_Events()
        //    {
        //        var testAggregate = new TestAggregate(Guid.NewGuid());

        //        // This should increment version by 1.
        //        testAggregate.ChangeAggregateData("New Data1");
        //        // This should increment version by 1.
        //        testAggregate.ChangeAggregateData("New Data2");
        //        // This should increment version by 1.
        //        testAggregate.ChangeAggregateData("New Data3");
        //        // This should increment version by 1.
        //        testAggregate.ChangeAggregateData("New Data4");

        //        testAggregate.ClearUncommitedDomainEvents();

        //        // Should have 0 uncommitted events since it is cleared.
        //        Assert.Equal(0, testAggregate.GetUncommitedDomainEvents().DomainEventCount);
        //    }
        //}
    }
}
