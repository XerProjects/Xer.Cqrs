using System;

namespace Xer.Cqrs.EventSourcing.Tests.Mocks
{
    public class TestAggregateCreatedEvent : DomainEvent
    {
        public string Data { get; }

        public TestAggregateCreatedEvent(Guid aggregateId, string data)
            : base(aggregateId)
        {
            Data = data;
        }
    }

    public class OperationExecutedEvent : DomainEvent
    {
        public virtual string Operation { get; }

        public OperationExecutedEvent(Guid testAggregateId, int aggregateVersion, string operation)
            : base(testAggregateId, aggregateVersion)
        {
            Operation = operation;
        }
    }

    public class DelayTriggeredEvent : DomainEvent
    {
        public string Operation => $"{GetType().Name}: Triggering delay for {DelayInMilliseconds} milliseconds.";
        public int DelayInMilliseconds { get; }

        public DelayTriggeredEvent(Guid testAggregateId, int aggregateVersion, int delayInMilliseconds) 
            : base(testAggregateId, aggregateVersion)
        {
            DelayInMilliseconds = delayInMilliseconds;
        }
    }

    public class ExceptionTriggeredEvent : DomainEvent
    {
        public string Operation => $"{GetType().Name}: Triggering exception.";

        public ExceptionTriggeredEvent(Guid testAggregateId, int aggregateVersion) 
            : base(testAggregateId, aggregateVersion)
        {
        }
    }
}
