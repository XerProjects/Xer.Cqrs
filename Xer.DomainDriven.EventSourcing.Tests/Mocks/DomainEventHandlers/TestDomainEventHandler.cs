using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xer.DomainDriven.EventSourcing.DomainEvents;
using Xer.DomainDriven.EventSourcing.Tests.Mocks.DomainEvents;
using Xunit.Abstractions;

namespace Xer.DomainDriven.EventSourcing.Tests.Mocks.DomainEventHandlers
{
    public class TestDomainEventHandler : IDomainEventHandler<TestAggregateCreated>,
                                          IDomainEventAsyncHandler<TestAggregateModified>
    {
        private readonly ITestOutputHelper _testOutput;

        public int NumberOfTimesInvoked { get; private set; }

        public TestDomainEventHandler(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        public void Handle(TestAggregateCreated domainEvent)
        {
            _testOutput.WriteLine($"{GetType().Name} has handled {domainEvent.GetType()}");
            NumberOfTimesInvoked++;
        }

        public Task HandleAsync(TestAggregateModified domainEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(domainEvent.ModifiedData == "Throw")
            {
                throw new Exception("Exception is triggered.");
            }

            _testOutput.WriteLine($"{GetType().Name} has async handled {domainEvent.GetType()}");
            NumberOfTimesInvoked++;

            return Task.CompletedTask;
        }
    }
}
