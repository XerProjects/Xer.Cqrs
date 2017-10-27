using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xer.Cqrs.Events;
using Xer.Cqrs.Events.Publishers;
using Xer.Cqrs.Events.Resolvers;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using System.Threading;
using System.Reflection;
using Xunit.Abstractions;
using Xer.Cqrs.Events.Registrations;
using System.Linq;

namespace Xer.Cqrs.Tests.Events
{
    public class EventPublisherTests
    {
        public class PublishMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public PublishMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public async Task Publish_Should_Invoke_All_Registered_Event_Handlers_In_Registration()
            {
                var registration = new EventHandlerRegistration();
                registration.Register(() => new TestEventAsyncHandler1(_testOutputHelper));
                registration.Register(() => new TestEventAsyncHandler2(_testOutputHelper));
                registration.Register(() => new TestEventAsyncHandler3(_testOutputHelper));
                registration.Register(() => new TestEventHandler1(_testOutputHelper));
                registration.Register(() => new TestEventHandler2(_testOutputHelper));
                registration.Register(() => new TestEventHandler3(_testOutputHelper));
                
                var publisher = new EventPublisher(registration);

                publisher.OnError += (eventHandler, ex) =>
                {
                    _testOutputHelper.WriteLine(ex.Message);
                };

                await publisher.PublishAsync(new TestEvent());
            }

            [Fact]
            public async Task Publish_Should_Invoke_All_Registered_Event_Handlers_In_Container()
            {
                var container = new Container();
                container.RegisterCollection(typeof(IEventAsyncHandler<>), typeof(TestEvent).Assembly);
                container.RegisterCollection(typeof(IEventHandler<>), typeof(TestEvent).Assembly);
                container.Register(() => _testOutputHelper);
                
                var adapter = new SimpleInjectorContainerAdapter(container);
                var eventHandlerResolver = new ContainerEventHandlerResolver(adapter);
                var publisher = new EventPublisher(eventHandlerResolver);

                publisher.OnError += (eventHandler, ex) =>
                {
                    _testOutputHelper.WriteLine(ex.Message);
                };
                
                await publisher.PublishAsync(new TestEvent());
            }
        }
    }

    public class TestEvent : IEvent
    {

    }

    public abstract class TestEventAsyncHandlerBase : IEventAsyncHandler<TestEvent>
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestEventAsyncHandlerBase(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public virtual Task HandleAsync(TestEvent @event, CancellationToken cancellationToken = default(CancellationToken))
        {
            _testOutputHelper.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event.");

            return Task.Delay(5000);
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
            throw new Exception($"Exception at { GetType().Name }");
        }
    }

    public class TestEventAsyncHandler3 : TestEventAsyncHandlerBase
    {
        public TestEventAsyncHandler3(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }

    public abstract class TestEventHandlerBase : IEventHandler<TestEvent>
    {
        private ITestOutputHelper _testOutputHelper;

        public TestEventHandlerBase(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public virtual void Handle(TestEvent @event)
        {
            _testOutputHelper.WriteLine($"{GetType().Name} handled {@event.GetType().Name} event.");
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
            throw new Exception($"Exception at { GetType().Name }");
        }
    }

    public class TestEventHandler3 : TestEventHandlerBase
    {
        public TestEventHandler3(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
