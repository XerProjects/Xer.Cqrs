using System;

namespace Xer.Cqrs.EventSourcing.Tests.Mocks
{
    #region TestAggregate

    public class TestAggregate : EventSourcedAggregate<Guid>
    {
        public string LastExecutedOperation { get; private set; }

        public TestAggregate(Guid aggregateId)
            : base(aggregateId)
        {
            ApplyChange(new TestAggregateCreatedEvent(aggregateId, string.Empty));
        }

        public TestAggregate(IDomainEventStream<Guid> history) 
            : base(history)
        {
        }

        public void ExecuteSomeOperation(string operation)
        {
            ApplyChange(new OperationExecutedEvent(Id, NextExpectedVersion, operation));
        }

        public void TriggerExceptionOnEventHandler()
        {
            ApplyChange(new ExceptionTriggeredEvent(Id, NextExpectedVersion));
        }

        public void TriggerDelayOnEventHandler(int milliSeconds)
        {
            ApplyChange(new DelayTriggeredEvent(Id, NextExpectedVersion, milliSeconds));
        }

        protected override void RegisterDomainEventAppliers(DomainEventApplierRegistration applierRegistration)
        {
            applierRegistration.RegisterApplierFor<TestAggregateCreatedEvent>(OnTestAggregateCreated);
            applierRegistration.RegisterApplierFor<OperationExecutedEvent>(OnOperationExecuted);
            applierRegistration.RegisterApplierFor<DelayTriggeredEvent>(OnDelayTriggered);
            applierRegistration.RegisterApplierFor<ExceptionTriggeredEvent>(OnExceptionTriggered);
        }

        private void OnTestAggregateCreated(TestAggregateCreatedEvent domainEvent)
        {
            Id = domainEvent.AggregateId;
            LastExecutedOperation = string.Empty;
        }

        private void OnOperationExecuted(OperationExecutedEvent domainEvent)
        {
            LastExecutedOperation = domainEvent.Operation;
        }

        private void OnDelayTriggered(DelayTriggeredEvent domainEvent)
        {
            LastExecutedOperation = domainEvent.Operation;
        }

        private void OnExceptionTriggered(ExceptionTriggeredEvent domainEvent)
        {
            LastExecutedOperation = domainEvent.Operation;
        }
    }

    #endregion TestAggregate
}
