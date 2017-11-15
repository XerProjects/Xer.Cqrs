using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static Xer.Cqrs.EventSourcing.Tests.Mocks.TestAggregateOperationExecuted;

namespace Xer.Cqrs.EventSourcing.Tests.Mocks.DomainEventHandlers
{
    #region TestDomainEventHandler

    public class TestDomainEventHandler : IDomainEventHandler<TestAggregateCreated>,
                                          IDomainEventAsyncHandler<TestAggregateOperationExecuted<int>>,
                                          IDomainEventAsyncHandler<TestAggregateOperationExecuted>
    {
        private readonly List<IDomainEvent> _handledEvents = new List<IDomainEvent>();
        private readonly ITestOutputHelper _testOutput;

        public IReadOnlyCollection<IDomainEvent> HandledEvents => _handledEvents.AsReadOnly();

        public TestDomainEventHandler(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        public void Handle(TestAggregateCreated domainEvent)
        {
            handle(domainEvent);
        }

        public Task HandleAsync(TestAggregateOperationExecuted domainEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            handleAsync(domainEvent);

            if (domainEvent.Operation == Operations.ThrowException)
            {
                throw new TestDomainEventHandlerException();
            }

            return Task.CompletedTask;
        }

        public async Task HandleAsync(TestAggregateOperationExecuted<int> domainEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            handleAsync(domainEvent);

            if (domainEvent.Operation == Operations.Delay)
            {
                _testOutput.WriteLine($"Delaying for {domainEvent.Data} milliseconds.");

                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromMilliseconds(domainEvent.Data), cancellationToken);
            }
        }

        private void handleAsync<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
        {
            _testOutput.WriteLine($"{DateTime.Now}: {GetType().Name} has handled {domainEvent.GetType()}. asynchronously");
            _handledEvents.Add(domainEvent);
        }

        private void handle<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
        {
            _testOutput.WriteLine($"{DateTime.Now}: {GetType().Name} has handled {domainEvent.GetType()}.");
            _handledEvents.Add(domainEvent);
        }
    }

    public class TestDomainEventHandlerException : Exception
    {
        public TestDomainEventHandlerException()
        {
        }
    }

    #endregion TestDomainEventHandler
}
