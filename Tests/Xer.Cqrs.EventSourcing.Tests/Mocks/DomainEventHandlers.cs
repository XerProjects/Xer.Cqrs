using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventSourcing.DomainEvents;
using Xunit.Abstractions;

namespace Xer.Cqrs.EventSourcing.Tests.Mocks.DomainEventHandlers
{
    #region TestDomainEventHandler

    public class TestDomainEventHandler : IDomainEventHandler<TestAggregateCreated>,
                                          IDomainEventAsyncHandler<TestAggregateModified>
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
            _testOutput.WriteLine($"{GetType().Name} has handled {domainEvent.GetType()}");
            _handledEvents.Add(domainEvent);
        }

        public Task HandleAsync(TestAggregateModified domainEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (domainEvent.ModifiedData == "Throw")
            {
                throw new Exception("Exception is triggered.");
            }

            _testOutput.WriteLine($"{GetType().Name} has async handled {domainEvent.GetType()}");
            _handledEvents.Add(domainEvent);

            return Task.CompletedTask;
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
