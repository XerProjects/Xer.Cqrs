using System;

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

    public class TestAggregateOperationExecuted : DomainEvent
    {
        public string Operation { get; }

        public TestAggregateOperationExecuted(Guid testAggregateId, int aggregateVersion, string operation)
            : base(testAggregateId, aggregateVersion)
        {
            Operation = operation;
        }

        public class Operations
        {
            public const string ThrowException = nameof(ThrowException);
            public const string Delay = nameof(Delay);
        }
    }

    public class TestAggregateOperationExecuted<TData> : TestAggregateOperationExecuted
    {
        public TData Data { get; }

        public TestAggregateOperationExecuted(Guid testAggregateId, int aggregateVersion, string operation, TData data) 
            : base(testAggregateId, aggregateVersion, operation)
        {
            Data = data;
        }
    }

    #endregion TestAggregateModified
}
