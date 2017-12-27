using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Registrations;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Events
{
    public class EventHandlerDelegateTests
    {
        public class InvokeMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public InvokeMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public async Task Should_Invoke_The_Actual_Registered_Command_Handler()
            {
                var eventHandler = new TestEventHandler1(_testOutputHelper);

                var registration = new EventHandlerRegistration();
                registration.Register(() => (IEventHandler<TestEvent1>)eventHandler);

                IEnumerable<EventHandlerDelegate> eventHandlerDelegates = registration.ResolveEventHandlers<TestEvent1>();

                Assert.NotNull(eventHandlerDelegates);
                Assert.NotEmpty(eventHandlerDelegates);

                // Invoke.
                await eventHandlerDelegates.First().Invoke(new TestEvent1());

                // Check if actual command handler instance was invoked.
                Assert.Equal(1, eventHandler.HandledEvents.Count);
                Assert.Contains(eventHandler.HandledEvents, e => e is TestEvent1);
            }

            [Fact]
            public Task Should_Check_For_Correct_Command_Type()
            {
                return Assert.ThrowsAnyAsync<ArgumentException>(async () =>
                {
                    var eventHandler = new TestEventHandler1(_testOutputHelper);

                    var registration = new EventHandlerRegistration();
                    registration.Register(() => (IEventAsyncHandler<TestEvent1>)eventHandler);

                    IEnumerable<EventHandlerDelegate> eventHandlerDelegates = registration.ResolveEventHandlers<TestEvent1>();

                    Assert.NotNull(eventHandlerDelegates);
                    Assert.NotEmpty(eventHandlerDelegates);

                    try
                    {
                        // This delegate handles TestEvent1, but was passed in a TestEvent2.
                        await eventHandlerDelegates.First().Invoke(new TestEvent2());
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }
    }
}
