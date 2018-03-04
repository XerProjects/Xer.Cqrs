using SimpleInjector;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Resolvers;
using Xer.Cqrs.EventStack.Tests.Entities;
using Xunit;
using Xunit.Abstractions;
using Xer.Delegator;
using System.Threading.Tasks;
using FluentAssertions;

namespace Xer.Cqrs.EventStack.Tests.Events.Registration
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
            public async Task ShouldResolverAllEventHandlersFromContainer()
            {
                Assembly assembly = typeof(TestEvent1).Assembly;

                // Register all event handlers.
                var containerWithEventHandlers = new Container();
                containerWithEventHandlers.Register(() => _outputHelper);
                containerWithEventHandlers.RegisterCollection(typeof(IEventAsyncHandler<>), assembly);
                containerWithEventHandlers.RegisterCollection(typeof(IEventHandler<>), assembly);

                // Get all event handlers from above container and register in this new container as singletons.
                // Register event handlers as singletons so that we can verify HandledEvents property.
                var container = new Container();
                container.Register(() => _outputHelper);
                container.RegisterCollection<IEventAsyncHandler<TestEvent1>>(containerWithEventHandlers.GetAllInstances<IEventAsyncHandler<TestEvent1>>().ToList());
                container.RegisterCollection<IEventHandler<TestEvent1>>(containerWithEventHandlers.GetAllInstances<IEventHandler<TestEvent1>>().ToList());

                var eventHandlerResolver = new ContainerEventHandlerResolver(new SimpleInjectorContainerAdapter(container));

                MessageHandlerDelegate eventHandlerDelegate = eventHandlerResolver.ResolveMessageHandler(typeof(TestEvent1));

                await eventHandlerDelegate.Invoke(new TestEvent1());

                // Get all handlers in assembly.
                var eventAsyncHandlers = container.GetAllInstances<IEventAsyncHandler<TestEvent1>>().Cast<TestEventHandler>().ToList();
                var eventSyncHandlers = container.GetAllInstances<IEventHandler<TestEvent1>>().Cast<TestEventHandler>().ToList();
                
                int totalEventHandlerCount = eventAsyncHandlers.Count + eventSyncHandlers.Count;

                int totalEventsHandledCount = eventAsyncHandlers.Sum(e => e.HandledEvents.Count) + 
                                              eventSyncHandlers.Sum(e => e.HandledEvents.Count);

                // Important Note: This should equal all handlers in this assembly.
                totalEventHandlerCount.Should().Be(totalEventsHandledCount);
            }

            [Fact]
            public async Task ShouldNotThrowIfNoEventHandlerIsRegisteredInContainer()
            {
                var container = new Container();
                var adapter = new SimpleInjectorContainerAdapter(container);
                var eventHandlerResolver = new ContainerEventHandlerResolver(adapter);
                var delegator = new EventDelegator(eventHandlerResolver);

                await delegator.SendAsync(new TestEvent1());
            }
        }

        #endregion ResolveMessageHandler Method
    }
}
