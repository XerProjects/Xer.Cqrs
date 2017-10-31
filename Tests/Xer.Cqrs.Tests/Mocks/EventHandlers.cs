using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Events;
using Xer.Cqrs.Events.Attributes;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Mocks
{
    #region Async Event Handlers

    public abstract class TestEventAsyncHandlerBase : IEventAsyncHandler<TestEvent>,
                                                      IEventAsyncHandler<TriggerLongRunningEvent>
    {
        private readonly List<IEvent> _handledEvents = new List<IEvent>();
        private readonly ITestOutputHelper _testOutputHelper;

        public IReadOnlyCollection<IEvent> HandledEvents => _handledEvents.AsReadOnly();

        public TestEventAsyncHandlerBase(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public virtual Task HandleAsync(TestEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            _testOutputHelper.WriteLine($"{DateTime.Now}: {GetType().Name} handled {@event.GetType().Name} event.");
            _handledEvents.Add(@event);

            return Task.CompletedTask;
        }

        public Task HandleAsync(TriggerLongRunningEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            _testOutputHelper.WriteLine($"{DateTime.Now}: {GetType().Name} handled {@event.GetType().Name} event.");
            _handledEvents.Add(@event);

            return Task.Delay(@event.Milliseconds, cancellationToken);
        }
    }

    public class TestEventAsyncHandler1 : TestEventAsyncHandlerBase
    {
        public TestEventAsyncHandler1(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }

    public class TestEventAsyncHandler2 : TestEventAsyncHandlerBase
    {
        public TestEventAsyncHandler2(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        public override Task HandleAsync(TestEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            base.HandleAsync(@event, cancellationToken);

            throw new TestEventHandlerException($"Exception at { GetType().Name }.");
        }
    }

    public class TestEventAsyncHandler3 : TestEventAsyncHandlerBase
    {
        public TestEventAsyncHandler3(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }

    #endregion Async Event Handlers

    #region Sync Event Handlers

    public abstract class TestEventHandlerBase : IEventHandler<TestEvent>,
                                                 IEventHandler<TriggerLongRunningEvent>
    {
        private readonly List<IEvent> _handledEvents = new List<IEvent>();
        private readonly ITestOutputHelper _testOutputHelper;

        public IReadOnlyCollection<IEvent> HandledEvents => _handledEvents.AsReadOnly();

        public TestEventHandlerBase(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public virtual void Handle(TestEvent @event)
        {
            _testOutputHelper.WriteLine($"{DateTime.Now}: {GetType().Name} handled {@event.GetType().Name} event.");
            _handledEvents.Add(@event);
        }

        public virtual void Handle(TriggerLongRunningEvent @event)
        {
            _testOutputHelper.WriteLine($"{DateTime.Now}: {GetType().Name} handled {@event.GetType().Name} event.");
            _handledEvents.Add(@event);
            Task.Delay(@event.Milliseconds).GetAwaiter().GetResult();
        }
    }

    public class TestEventHandler1 : TestEventHandlerBase
    {
        public TestEventHandler1(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }

    public class TestEventHandler2 : TestEventHandlerBase
    {
        public TestEventHandler2(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        public override void Handle(TestEvent @event)
        {
            base.Handle(@event);

            throw new TestEventHandlerException($"Exception at { GetType().Name }.");
        }
    }

    public class TestEventHandler3 : TestEventHandlerBase
    {
        public TestEventHandler3(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }

    #endregion Sync Event Handlers

    #region Attribute Event Handlers

    public abstract class TestAttributedEventHandlerBase
    {
        protected List<IEvent> InternalHandledEvents { get; } = new List<IEvent>();
        protected ITestOutputHelper TestOutputHelper { get; }

        public IReadOnlyCollection<IEvent> HandledEvents => InternalHandledEvents.AsReadOnly();

        public TestAttributedEventHandlerBase(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
        }
    }

    public class TestAttributedEventHandler1 : TestAttributedEventHandlerBase
    {
        public TestAttributedEventHandler1(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
        
        [EventHandler]
        public void Handle(TestEvent testEvent)
        {
            TestOutputHelper.WriteLine($"{GetType().Name} handled {testEvent.GetType().Name} event.");
            InternalHandledEvents.Add(testEvent);
        }

        [EventHandler]
        public Task Handle(TriggerLongRunningEvent @event, CancellationToken cancellationToken)
        {
            TestOutputHelper.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event.");
            InternalHandledEvents.Add(@event);

            return Task.Delay(@event.Milliseconds, cancellationToken);
        }
    }

    public class TestAttributedEventHandler2 : TestAttributedEventHandlerBase
    {
        public TestAttributedEventHandler2(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [EventHandler]
        public void Handle(TestEvent @event)
        {
            TestOutputHelper.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event.");
            InternalHandledEvents.Add(@event);

            throw new TestEventHandlerException($"Exception at { GetType().Name }.{nameof(Handle)}({nameof(TestEvent)}).");
        }

        [EventHandler]
        public Task HandleAsync(TestEvent @event)
        {
            TestOutputHelper.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event asynchronously.");
            InternalHandledEvents.Add(@event);

            throw new TestEventHandlerException($"Exception at { GetType().Name }.{nameof(HandleAsync)}({nameof(TestEvent)}).");
        }

        [EventHandler]
        public Task Handle(TriggerLongRunningEvent @event, CancellationToken cancellationToken)
        {
            TestOutputHelper.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event.");
            InternalHandledEvents.Add(@event);

            return Task.Delay(@event.Milliseconds, cancellationToken);
        }
    }

    public class TestAttributedEventHandler3 : TestAttributedEventHandlerBase
    {
        public TestAttributedEventHandler3(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [EventHandler]
        public void Handle(TestEvent @event)
        {
            TestOutputHelper.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event.");
            InternalHandledEvents.Add(@event);
        }

        [EventHandler]
        public Task Handle(TriggerLongRunningEvent @event, CancellationToken cancellationToken)
        {
            TestOutputHelper.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event.");
            InternalHandledEvents.Add(@event);

            return Task.Delay(@event.Milliseconds, cancellationToken);
        }
    }

    #endregion Attribute Event Handlers

    public class TestEventHandlerException : Exception
    {
        public TestEventHandlerException()
        {
        }

        public TestEventHandlerException(string message) : base(message)
        {
        }
    }
}
