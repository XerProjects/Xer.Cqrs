using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Tests.Entities;
using Xer.Delegator;
using Xer.Delegator.Registrations;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.EventStack.Tests.Events.Registration
{
    public class BasicRegistrationTests
    {
        #region RegisterEventHandler Method

        public class RegisterEventHandlerMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public RegisterEventHandlerMethod(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            [Fact]
            public async Task ShouldRegisterAllEventHandlers()
            {
                // 6 handlers.
                var asyncHandler1 = new TestEventHandler(_outputHelper);
                var asyncHandler2 = new TestEventHandler(_outputHelper);
                var asyncHandler3 = new TestEventHandler(_outputHelper);
                var handler1 = new TestEventHandler(_outputHelper);
                var handler2 = new TestEventHandler(_outputHelper);
                var handler3 = new TestEventHandler(_outputHelper);

                var registration = new MultiMessageHandlerRegistration();
                registration.RegisterEventHandler<TestEvent1>(() => asyncHandler1);
                registration.RegisterEventHandler<TestEvent1>(() => asyncHandler2);
                registration.RegisterEventHandler<TestEvent1>(() => asyncHandler3);
                registration.RegisterEventHandler<TestEvent1>(() => handler1.AsEventSyncHandler<TestEvent1>());
                registration.RegisterEventHandler<TestEvent1>(() => handler2.AsEventSyncHandler<TestEvent1>());
                registration.RegisterEventHandler<TestEvent1>(() => handler3.AsEventSyncHandler<TestEvent1>());

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                MessageHandlerDelegate eventHandlerDelegate = resolver.ResolveMessageHandler(typeof(TestEvent1));

                await eventHandlerDelegate.Invoke(new TestEvent1());

                int totalEventsHandledCount = asyncHandler1.HandledEvents.Count + 
                                              asyncHandler2.HandledEvents.Count + 
                                              asyncHandler3.HandledEvents.Count +
                                              handler1.HandledEvents.Count + 
                                              handler2.HandledEvents.Count + 
                                              handler3.HandledEvents.Count;

                totalEventsHandledCount.Should().Be(6);
            }
        }

        #endregion RegisterEventHandler Method
    }
}
