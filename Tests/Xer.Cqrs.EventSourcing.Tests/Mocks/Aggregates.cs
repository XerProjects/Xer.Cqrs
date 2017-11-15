using System;

namespace Xer.Cqrs.EventSourcing.Tests.Mocks
{
    #region TestAggregate

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

        public void ExecuteSomeOperation(string newData)
        {
            ApplyChange(new TestAggregateOperationExecuted(Id, NextExpectedVersion, newData));
        }

        public void ThrowExceptionOnEventHandler()
        {
            ApplyChange(new TestAggregateOperationExecuted(Id, NextExpectedVersion, TestAggregateOperationExecuted.Operations.ThrowException));
        }

        public void DelayOnEventHandler(int milliSeconds)
        {
            ApplyChange(new TestAggregateOperationExecuted<int>(Id, NextExpectedVersion, TestAggregateOperationExecuted.Operations.Delay, milliSeconds));
        }

        protected override void RegisterDomainEventAppliers(DomainEventApplierRegistration applierRegistration)
        {
            applierRegistration.RegisterApplierFor<TestAggregateCreated>(OnTestAggregateCreated);
            applierRegistration.RegisterApplierFor<TestAggregateOperationExecuted>(OnTestAggregateOperationExecuted);
            applierRegistration.RegisterApplierFor<TestAggregateOperationExecuted<int>>(OnTestAggregateOperationExecuted);
        }

        private void OnTestAggregateCreated(TestAggregateCreated domainEvent)
        {
            Id = domainEvent.AggregateId;
            AggregateData = domainEvent.Data;
        }

        private void OnTestAggregateOperationExecuted(TestAggregateOperationExecuted domainEvent)
        {
            AggregateData = domainEvent.Operation;
        }

        private void OnTestAggregateOperationExecuted(TestAggregateOperationExecuted<int> domainEvent)
        {
            AggregateData = domainEvent.Data.ToString();
        }
    }

    #endregion TestAggregate
}
