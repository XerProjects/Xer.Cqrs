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

    public abstract class TestEventAsyncHandlerBase : IEventAsyncHandler<TestEvent1>,
                                                      IEventAsyncHandler<TestEvent2>,
                                                      IEventAsyncHandler<TestEvent3>,
                                                      IEventAsyncHandler<TriggerLongRunningEvent>
    {
        private readonly List<IEvent> _handledEvents = new List<IEvent>();
        private readonly ITestOutputHelper _testOutputHelper;

        public IReadOnlyCollection<IEvent> HandledEvents => _handledEvents.AsReadOnly();

        public TestEventAsyncHandlerBase(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public virtual Task HandleAsync(TestEvent1 @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(@event);

            return Task.CompletedTask;
        }

        public virtual Task HandleAsync(TestEvent2 @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(@event);

            return Task.CompletedTask;
        }

        public virtual Task HandleAsync(TestEvent3 @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(@event);

            return Task.CompletedTask;
        }

        public virtual Task HandleAsync(TriggerLongRunningEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(@event);

            return Task.Delay(@event.DurationInMilliseconds, cancellationToken);
        }

        private void handle<TEvent>(TEvent @event) where TEvent : IEvent
        {
            _testOutputHelper.WriteLine($"{DateTime.Now}: {GetType().Name} handled {@event.GetType().Name} event asynchronously.");
            _handledEvents.Add(@event);
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

        public override Task HandleAsync(TestEvent1 @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            base.HandleAsync(@event, cancellationToken);

            return Task.FromException(new TestEventHandlerException($"Exception at { GetType().Name }."));
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

    public abstract class TestEventHandlerBase : IEventHandler<TestEvent1>,
                                                 IEventHandler<TestEvent2>,
                                                 IEventHandler<TestEvent3>,
                                                 IEventHandler<TriggerLongRunningEvent>
    {
        private readonly List<IEvent> _handledEvents = new List<IEvent>();
        private readonly ITestOutputHelper _testOutputHelper;

        public IReadOnlyCollection<IEvent> HandledEvents => _handledEvents.AsReadOnly();

        public TestEventHandlerBase(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public virtual void Handle(TestEvent1 @event)
        {
            handle(@event);
        }

        public void Handle(TestEvent2 @event)
        {
            handle(@event);
        }

        public void Handle(TestEvent3 @event)
        {
            handle(@event);
        }

        public virtual void Handle(TriggerLongRunningEvent @event)
        {
            handle(@event);
            Task.Delay(@event.DurationInMilliseconds).GetAwaiter().GetResult();
        }

        private void handle<TEvent>(TEvent @event) where TEvent : IEvent
        {
            _testOutputHelper.WriteLine($"{DateTime.Now}: {GetType().Name} handled {@event.GetType().Name} event.");
            _handledEvents.Add(@event);
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

        public override void Handle(TestEvent1 @event)
        {
            base.Handle(@event);

            throw new TestEventHandlerException($"This is a triggered post-processing exception at { GetType().Name }.");
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

        protected void Handle<TEvent>(TEvent @event) where TEvent : IEvent
        {
            TestOutputHelper.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event.");
            InternalHandledEvents.Add(@event);
        }

        protected void HandleAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            TestOutputHelper.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event asynchronously.");
            InternalHandledEvents.Add(@event);
        }
    }

    public class TestAttributedEventHandler1 : TestAttributedEventHandlerBase
    {
        public TestAttributedEventHandler1(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
        
        [EventHandler]
        public void Handle(TestEvent1 @event)
        {
            base.Handle(@event);
        }

        [EventHandler]
        public void Handle(TestEvent2 @event)
        {
            base.Handle(@event);
        }

        [EventHandler]
        public void Handle(TestEvent3 @event)
        {
            base.Handle(@event);
        }

        [EventHandler]
        public Task HandleAsync(TestEvent1 @event)
        {
            base.HandleAsync(@event);
            return Task.CompletedTask;
        }

        [EventHandler]
        public Task HandleAsync(TestEvent2 @event)
        {
            base.HandleAsync(@event);
            return Task.CompletedTask;
        }

        [EventHandler]
        public Task HandleAsync(TestEvent3 @event)
        {
            base.HandleAsync(@event);
            return Task.CompletedTask;
        }

        [EventHandler]
        public Task HandleAsync(TriggerLongRunningEvent @event, CancellationToken cancellationToken)
        {
            base.HandleAsync(@event);
            return Task.Delay(@event.DurationInMilliseconds, cancellationToken);
        }
    }

    public class TestAttributedEventHandler2 : TestAttributedEventHandlerBase
    {
        public TestAttributedEventHandler2(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [EventHandler]
        public void HandleWithException(TestEvent1 @event)
        {
            base.Handle(@event);
            throw new TestEventHandlerException($"This is a triggered post-processing exception at { GetType().Name }.{nameof(Handle)}({nameof(TestEvent1)}).");
        }

        [EventHandler]
        public void Handle(TestEvent2 @event)
        {
            base.Handle(@event);
        }

        [EventHandler]
        public void Handle(TestEvent3 @event)
        {
            base.Handle(@event);
        }

        [EventHandler]
        public Task HandleAsync(TestEvent1 @event)
        {
            base.HandleAsync(@event);
            
            return Task.FromException(new TestEventHandlerException($"This is a triggered post-processing exception at { GetType().Name }.{nameof(HandleAsync)}({nameof(TestEvent1)})."));
        }

        [EventHandler]
        public Task HandleAsync(TestEvent2 @event)
        {
            base.HandleAsync(@event);
            return Task.CompletedTask;
        }

        [EventHandler]
        public Task HandleAsync(TestEvent3 @event)
        {
            base.HandleAsync(@event);
            return Task.CompletedTask;
        }

        [EventHandler]
        public Task HandleAsync(TriggerLongRunningEvent @event, CancellationToken cancellationToken)
        {
            base.HandleAsync(@event);
            return Task.Delay(@event.DurationInMilliseconds, cancellationToken);
        }
    }

    public class TestAttributedEventHandler3 : TestAttributedEventHandlerBase
    {
        public TestAttributedEventHandler3(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [EventHandler]
        public void Handle(TestEvent1 @event)
        {
            base.Handle(@event);
        }

        [EventHandler]
        public void Handle(TestEvent2 @event)
        {
            base.Handle(@event);
        }

        [EventHandler]
        public void Handle(TestEvent3 @event)
        {
            base.Handle(@event);
        }

        [EventHandler]
        public Task HandleAsync(TestEvent1 @event)
        {
            base.HandleAsync(@event);
            return Task.CompletedTask;
        }

        [EventHandler]
        public Task HandleAsync(TestEvent2 @event)
        {
            base.HandleAsync(@event);
            return Task.CompletedTask;
        }

        [EventHandler]
        public Task HandleAsync(TestEvent3 @event)
        {
            base.HandleAsync(@event);
            return Task.CompletedTask;
        }

        [EventHandler]
        public Task HandleAsync(TriggerLongRunningEvent @event, CancellationToken cancellationToken)
        {
            base.HandleAsync(@event);
            return Task.Delay(@event.DurationInMilliseconds, cancellationToken);
        }
    }

    #endregion Attribute Event Handlers

    public class TestEventHandlerException : Exception
    {
        public TestEventHandlerException() { }
        public TestEventHandlerException(string message) : base(message) { }
    }
}
