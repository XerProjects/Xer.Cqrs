using System;
using Xer.Cqrs.EventSourcing.DomainEvents;
using Xer.Cqrs.EventSourcing.Tests.Mocks.DomainEvents;

namespace Xer.Cqrs.EventSourcing.Tests.Mocks
{
    public class TestAggregate : EventSourcedAggregate
    {
        public string AggregateData { get; private set; }

        public TestAggregate(Guid aggregateId) 
            : base(aggregateId)
        {
            ApplyChange(new TestAggregateCreated(aggregateId, string.Empty));
        }

        public TestAggregate(DomainEventStream history) 
            : base(history)
        {
        }

        public void ChangeAggregateData(string newData)
        {
            ApplyChange(new TestAggregateModified(Id, Version + 1, newData));
        }

        public void ThrowExceptionOnEventHandler()
        {
            ApplyChange(new TestAggregateModified(Id, Version + 1, "Throw"));
        }

        protected override void RegisterDomainEventAppliers(DomainEventApplierRegistration applierRegistration)
        {
            applierRegistration.RegisterDomainEventApplier<TestAggregateCreated>(OnTestAggregateCreated);
            applierRegistration.RegisterDomainEventApplier<TestAggregateModified>(OnTestAggregateModified);
        }

        private void OnTestAggregateCreated(TestAggregateCreated domainEvent)
        {
            Id = domainEvent.AggregateId;
            AggregateData = domainEvent.Data;
        }

        private void OnTestAggregateModified(TestAggregateModified domainEvent)
        {
            AggregateData = domainEvent.ModifiedData;
        }
    }
}
