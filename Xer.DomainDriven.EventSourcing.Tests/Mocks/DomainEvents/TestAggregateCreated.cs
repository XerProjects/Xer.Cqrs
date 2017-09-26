using System;
using System.Collections.Generic;
using System.Text;
using Xer.DomainDriven.EventSourcing.DomainEvents;

namespace Xer.DomainDriven.EventSourcing.Tests.Mocks.DomainEvents
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
