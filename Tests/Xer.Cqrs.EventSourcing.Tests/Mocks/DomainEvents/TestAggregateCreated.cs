using System;
using Xer.Cqrs.EventSourcing.DomainEvents;

namespace Xer.Cqrs.EventSourcing.Tests.Mocks.DomainEvents
{
    public class TestAggregateCreated : DomainEvent
    {
        public string Data { get; }

        public TestAggregateCreated(Guid aggregateId, string data)
            : base(aggregateId)
        {
            Data = data;
        }
    }
}
