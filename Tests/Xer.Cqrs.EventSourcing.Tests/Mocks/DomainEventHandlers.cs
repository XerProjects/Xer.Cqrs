using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static Xer.Cqrs.EventSourcing.Tests.Mocks.OperationExecutedEvent;

namespace Xer.Cqrs.EventSourcing.Tests.Mocks.DomainEventHandlers
{
    #region TestDomainEventHandler

    public class TestDomainEventHandler : IDomainEventHandler<TestAggregateCreatedEvent>,
                                          IDomainEventAsyncHandler<DelayTriggeredEvent>,
                                          IDomainEventAsyncHandler<ExceptionTriggeredEvent>,
                                          IDomainEventAsyncHandler<OperationExecutedEvent>
    {
        private readonly List<IDomainEvent> _handledEvents = new List<IDomainEvent>();
        private readonly ITestOutputHelper _testOutput;

        public IReadOnlyCollection<IDomainEvent> HandledEvents => _handledEvents.AsReadOnly();

        public TestDomainEventHandler(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        public void Handle(TestAggregateCreatedEvent domainEvent)
        {
            handle(domainEvent);
        }

        public Task HandleAsync(OperationExecutedEvent domainEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            handleAsync(domainEvent);
            
            return Task.CompletedTask;
        }

        public Task HandleAsync(ExceptionTriggeredEvent domainEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            handleAsync(domainEvent);

            throw new TestAggregateDomainEventHandlerException();
        }

        public async Task HandleAsync(DelayTriggeredEvent domainEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            handleAsync(domainEvent);

            cancellationToken.ThrowIfCancellationRequested();
            _testOutput.WriteLine($"Delaying for {domainEvent.DelayInMilliseconds} milliseconds.");
            await Task.Delay(TimeSpan.FromMilliseconds(domainEvent.DelayInMilliseconds), cancellationToken);
        }

        private void handleAsync<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
        {
            _testOutput.WriteLine($"{DateTime.Now}: {GetType().Name} has handled {domainEvent.GetType()} asynchronously.");
            _handledEvents.Add(domainEvent);
        }

        private void handle<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
        {
            _testOutput.WriteLine($"{DateTime.Now}: {GetType().Name} has handled {domainEvent.GetType()}.");
            _handledEvents.Add(domainEvent);
        }
    }

    public class TestAggregateDomainEventHandlerException : Exception
    {
        public TestAggregateDomainEventHandlerException()
        {
        }
    }

    #endregion TestDomainEventHandler
}
