using System;
using Xer.Cqrs.EventSourcing.DomainEvents;

namespace Xer.Cqrs.EventSourcing.Tests.Mocks.DomainEvents
{
    public class TestAggregateModified : DomainEvent
    {
        public string ModifiedData { get; }

        public TestAggregateModified(Guid testAggregateId, int aggregateVersion, string modifiedData)
            : base(testAggregateId, aggregateVersion)
        {
            ModifiedData = modifiedData;
        }

        //public TestAggregateModified(TestAggregate aggregate, string data)
        //    : base(aggregate)
        //{
        //    ModifiedData = data ?? throw new ArgumentNullException(nameof(data));
        //}
    }
}
