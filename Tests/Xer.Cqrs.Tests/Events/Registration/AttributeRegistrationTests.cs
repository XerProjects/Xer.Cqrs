using System.Collections.Generic;
using System.Linq;
using Xer.Cqrs.Events;
using Xer.Cqrs.Events.Attributes;
using Xer.Cqrs.Events.Registrations;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Events.Registration
{
    public class AttributeRegistrationTests
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
            public void Should_Store_All_Event_Attributed_Handlers()
            {
                var attributedHandler1 = new TestAttributedEventHandler1(_testOutputHelper);
                var attributedHandler2 = new TestAttributedEventHandler2(_testOutputHelper);
                var attributedHandler3 = new TestAttributedEventHandler3(_testOutputHelper);

                var registration = new EventHandlerAttributeRegistration();
                registration.Register(() => attributedHandler1);
                registration.Register(() => attributedHandler2);
                registration.Register(() => attributedHandler3);

                IEnumerable<EventHandlerDelegate> eventHandlerDelegates = registration.ResolveEventHandlers<TestEvent1>();

                // Get all methods marked with [EventHandler] and receiving TestEvent as parameter.
                int eventHandler1MethodCount = attributedHandler1.GetType().GetMethods().Count(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(EventHandlerAttribute)) &&
                  m.GetParameters().Any(p => p.ParameterType == typeof(TestEvent1)));

                int eventHandler2MethodCount = attributedHandler2.GetType().GetMethods().Count(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(EventHandlerAttribute)) &&
                  m.GetParameters().Any(p => p.ParameterType == typeof(TestEvent1)));

                int eventHandler3MethodCount = attributedHandler3.GetType().GetMethods().Count(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(EventHandlerAttribute)) &&
                  m.GetParameters().Any(p => p.ParameterType == typeof(TestEvent1)));

                int totalEventHandlerMethodCount = eventHandler1MethodCount + eventHandler2MethodCount + eventHandler3MethodCount;

                Assert.Equal(totalEventHandlerMethodCount, eventHandlerDelegates.Count());
            }
        }

        #endregion Register Method
    }
}
