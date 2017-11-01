using SimpleInjector;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xer.Cqrs.Events;
using Xer.Cqrs.Events.Resolvers;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Events.Registration
{
    public class ContainerRegistrationTests
    {
        #region Register Method

        public class RegisterMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public RegisterMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public void Should_Resolve_All_Event_Handlers()
            {
                Assembly assembly = typeof(TestEvent1).Assembly;

                var container = new Container();
                container.RegisterCollection(typeof(IEventAsyncHandler<>), assembly);
                container.RegisterCollection(typeof(IEventHandler<>), assembly);
                container.Register(() => _testOutputHelper);

                var adapter = new SimpleInjectorContainerAdapter(container);
                var eventHandlerResolver = new ContainerEventHandlerResolver(adapter);

                IEnumerable<EventHandlerDelegate> eventHandlerDelegates = eventHandlerResolver.ResolveEventHandlers<TestEvent1>();

                // Get all handlers in assembly.
                int asyncHandlerCount = assembly.DefinedTypes.Count(t =>
                    !t.IsAbstract && !t.IsInterface &&
                    t.ImplementedInterfaces.Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventAsyncHandler<>)));

                int syncHandlerCount = assembly.DefinedTypes.Count(t =>
                    !t.IsAbstract && !t.IsInterface &&
                    t.ImplementedInterfaces.Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)));

                int totalHandlerCount = asyncHandlerCount + syncHandlerCount;

                // Important Note: This should equal all handlers in this assembly.
                Assert.Equal(totalHandlerCount, eventHandlerDelegates.Count());
            }
        }

        #endregion Register Method
    }
}
