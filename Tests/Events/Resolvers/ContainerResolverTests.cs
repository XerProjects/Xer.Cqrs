using SimpleInjector;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Resolvers;
using Xer.Cqrs.Tests.Entities;
using Xunit;
using Xunit.Abstractions;
using Xer.Delegator;
using System.Threading.Tasks;

namespace Xer.Cqrs.Tests.Events.Registration
{
    public class ContainerRegistrationTests
    {
        #region ResolveMessageHandler Method

        public class ResolveMessageHandlerMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public ResolveMessageHandlerMethod(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            [Fact]
            public async Task Should_Register_All_Event_Handlers()
            {
                Assembly assembly = typeof(TestEvent1).Assembly;

                var containerWithEventHandlers = new Container();
                containerWithEventHandlers.RegisterCollection(typeof(IEventAsyncHandler<>), assembly);
                containerWithEventHandlers.RegisterCollection(typeof(IEventHandler<>), assembly);
                containerWithEventHandlers.Register(() => _outputHelper);

                var container = new Container();
                // Register event handlers as singletons so that we can verify HandledEvents property.
                container.RegisterCollection<IEventAsyncHandler<TestEvent1>>(containerWithEventHandlers.GetAllInstances<IEventAsyncHandler<TestEvent1>>().ToList());
                // Register event handlers as singletons so that we can verify HandledEvents property.
                container.RegisterCollection<IEventHandler<TestEvent1>>(containerWithEventHandlers.GetAllInstances<IEventHandler<TestEvent1>>().ToList());
                container.Register(() => _outputHelper);

                var adapter = new SimpleInjectorContainerAdapter(container);
                var eventHandlerResolver = new ContainerEventHandlerResolver(adapter);

                MessageHandlerDelegate eventHandlerDelegate = eventHandlerResolver.ResolveMessageHandler(typeof(TestEvent1));

                await eventHandlerDelegate.Invoke(new TestEvent1());

                // Get all handlers in assembly.
                var asyncHandlers = container.GetAllInstances<IEventAsyncHandler<TestEvent1>>().Cast<TestEventHandler>().ToList();
                var syncHandlers = container.GetAllInstances<IEventHandler<TestEvent1>>().Cast<TestEventHandler>().ToList();
                int totalHandlerCount = asyncHandlers.Count + syncHandlers.Count;

                int asyncHandled = asyncHandlers.Sum(e => e.HandledEvents.Count);
                int syncHandled = syncHandlers.Sum(e => e.HandledEvents.Count);
                int totalHandledCount = asyncHandled + syncHandled;

                // Important Note: This should equal all handlers in this assembly.
                Assert.Equal(totalHandlerCount, totalHandledCount);
            }
        }

        #endregion ResolveMessageHandler Method
    }
}
