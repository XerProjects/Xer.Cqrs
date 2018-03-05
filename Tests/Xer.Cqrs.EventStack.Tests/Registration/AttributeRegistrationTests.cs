using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Attributes;
using Xer.Cqrs.EventStack.Tests.Entities;
using Xer.Delegator;
using Xer.Delegator.Registrations;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.EventStack.Tests.Events.Registration
{
    public class AttributeRegistrationTests
    {
        #region RegisterEventHandlerAttributes Method

        public class RegisterEventHandlerAttributesMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public RegisterEventHandlerAttributesMethod(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            [Fact]
            public async Task ShouldRegisterAllMethodsMarkedWithEventHandlerAttribute()
            {
                var attributedHandler1 = new TestAttributedEventHandler(_outputHelper);
                var attributedHandler2 = new TestAttributedEventHandler(_outputHelper);
                var attributedHandler3 = new TestAttributedEventHandler(_outputHelper);

                var registration = new MultiMessageHandlerRegistration();
                registration.RegisterEventHandlerAttributes(() => attributedHandler1);
                registration.RegisterEventHandlerAttributes(EventHandlerAttributeMethod.FromType<TestAttributedEventHandler>(() => attributedHandler2));
                registration.RegisterEventHandlerAttributes(EventHandlerAttributeMethod.FromType(typeof(TestAttributedEventHandler), () => attributedHandler3));
                
                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                MessageHandlerDelegate eventHandlerDelegate = resolver.ResolveMessageHandler(typeof(TestEvent1));

                // Get all methods marked with [EventHandler] and receiving TestEvent as parameter.
                int eventHandler1MethodCount = TestAttributedEventHandler.GetEventHandlerAttributeCountFor<TestEvent1>();
                int eventHandler2MethodCount = TestAttributedEventHandler.GetEventHandlerAttributeCountFor<TestEvent1>();
                int eventHandler3MethodCount = TestAttributedEventHandler.GetEventHandlerAttributeCountFor<TestEvent1>();

                await eventHandlerDelegate.Invoke(new TestEvent1());

                int totalEventHandlerMethodCount = eventHandler1MethodCount + eventHandler2MethodCount + eventHandler3MethodCount;
                int totalEventsHandledCount = attributedHandler1.HandledEvents.Count +  attributedHandler2.HandledEvents.Count + attributedHandler3.HandledEvents.Count;

                totalEventsHandledCount.Should().Be(totalEventHandlerMethodCount);
            }
        }

        #endregion RegisterEventHandlerAttributes Method
    }
}
