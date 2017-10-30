using System;
using Xer.Cqrs.EventSourcing.DomainEvents;

namespace Xer.Cqrs.EventSourcing.Tests.Mocks
{
    #region TestAggregateCreated

    public class TestAggregateCreated : DomainEvent
    {
        public string Data { get; }

        public TestAggregateCreated(Guid aggregateId, string data)
            : base(aggregateId)
        {
            Data = data;
        }
    }

    #endregion TestAggregateCreated

    #region TestAggregateModified

    public class TestAggregateModified : DomainEvent
    {
        public string ModifiedData { get; }

        public TestAggregateModified(Guid testAggregateId, int aggregateVersion, string modifiedData)
            : base(testAggregateId, aggregateVersion)
        {
            ModifiedData = modifiedData;
        }
    }

    #endregion TestAggregateModified
}
