using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Publishers;
using Xer.Cqrs.EventStack.Registrations;
using Xer.Cqrs.EventStack.Resolvers;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Events
{
    public class EventPublisherTests
    {
        #region PublishAsync Method

        public class PublishAsyncMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public PublishAsyncMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            #region Publish To Registered Event Handlers

            [Fact]
            public async Task Should_Invoke_All_Registered_Event_Handlers_In_Registration()
            {
                var asyncHandler1 = new TestEventAsyncHandler1(_testOutputHelper);
                var asyncHandler2 = new TestEventAsyncHandler2(_testOutputHelper);
                var asyncHandler3 = new TestEventAsyncHandler3(_testOutputHelper);
                var handler1 = new TestEventHandler1(_testOutputHelper);
                var handler2 = new TestEventHandler2(_testOutputHelper);
                var handler3 = new TestEventHandler3(_testOutputHelper);

                var registration = new EventHandlerRegistration();
                registration.Register<TestEvent1>(() => handler1);
                registration.Register<TestEvent1>(() => handler2);
                registration.Register<TestEvent1>(() => handler3);
                registration.Register<TestEvent1>(() => asyncHandler1);
                registration.Register<TestEvent1>(() => asyncHandler2);
                registration.Register<TestEvent1>(() => asyncHandler3);

                var publisher = new EventPublisher(registration);

                publisher.OnError += (@event, ex) =>
                {
                    _testOutputHelper.WriteLine(ex.Message);
                };

                await publisher.PublishAsync(new TestEvent1());

                // Event may not have yet been handled in background.
                await Task.Delay(500);

                // AsyncHandler1 should have 1 event.
                // 1. TestEvent1
                Assert.Equal(1, asyncHandler1.HandledEvents.Count);
                Assert.Contains(asyncHandler1.HandledEvents, (e) => e is TestEvent1);

                // AsyncHandler2 should have 1 event.
                // 1. TestEvent1
                Assert.Equal(1, asyncHandler2.HandledEvents.Count);
                Assert.Contains(asyncHandler2.HandledEvents, (e) => e is TestEvent1);

                // AsyncHandler3 should have 1 event.
                // 1. TestEvent1
                Assert.Equal(1, asyncHandler3.HandledEvents.Count);
                Assert.Contains(asyncHandler3.HandledEvents, (e) => e is TestEvent1);

                // Handler1 should have 1 event.
                // 1. TestEvent1
                Assert.Equal(1, handler1.HandledEvents.Count);
                Assert.Contains(handler1.HandledEvents, (e) => e is TestEvent1);

                // Handler2 should have 1 event.
                // 1. TestEvent1
                Assert.Equal(1, handler2.HandledEvents.Count);
                Assert.Contains(handler2.HandledEvents, (e) => e is TestEvent1);

                // Handler3 should have 1 event.
                // 1. TestEvent1
                Assert.Equal(1, handler3.HandledEvents.Count);
                Assert.Contains(handler3.HandledEvents, (e) => e is TestEvent1);
            }

            [Fact]
            public async Task Should_Invoke_All_Registered_Event_Handlers_In_Registration_For_Each_Events()
            {
                var asyncHandler1 = new TestEventAsyncHandler1(_testOutputHelper);
                var asyncHandler2 = new TestEventAsyncHandler2(_testOutputHelper);
                var asyncHandler3 = new TestEventAsyncHandler3(_testOutputHelper);
                var handler1 = new TestEventHandler1(_testOutputHelper);
                var handler2 = new TestEventHandler2(_testOutputHelper);
                var handler3 = new TestEventHandler3(_testOutputHelper);

                var registration = new EventHandlerRegistration();
                registration.Register<TestEvent1>(() => asyncHandler1);
                registration.Register<TestEvent1>(() => asyncHandler2);
                registration.Register<TestEvent1>(() => asyncHandler3);
                registration.Register<TestEvent1>(() => handler1);
                registration.Register<TestEvent1>(() => handler2);
                registration.Register<TestEvent1>(() => handler3);

                registration.Register<TestEvent2>(() => asyncHandler1);
                registration.Register<TestEvent2>(() => asyncHandler2);
                registration.Register<TestEvent2>(() => asyncHandler3);
                registration.Register<TestEvent2>(() => handler1);
                registration.Register<TestEvent2>(() => handler2);
                registration.Register<TestEvent2>(() => handler3);

                registration.Register<TestEvent3>(() => asyncHandler1);
                registration.Register<TestEvent3>(() => asyncHandler2);
                registration.Register<TestEvent3>(() => asyncHandler3);
                registration.Register<TestEvent3>(() => handler1);
                registration.Register<TestEvent3>(() => handler2);
                registration.Register<TestEvent3>(() => handler3);

                var publisher = new EventPublisher(registration);

                publisher.OnError += (@event, ex) =>
                {
                    _testOutputHelper.WriteLine(ex.Message);
                };

                await publisher.PublishAsync(new List<IEvent> { new TestEvent1(), new TestEvent2(), new TestEvent3() });

                // Event may not have yet been handled in background.
                await Task.Delay(500);

                // AsyncHandler1 should have 3 events.
                // 1. TestEvent1
                // 2. TestEvent2
                // 3. TestEvent3
                Assert.Equal(3, asyncHandler1.HandledEvents.Count);
                Assert.Contains(asyncHandler1.HandledEvents, (e) => e is TestEvent1);
                Assert.Contains(asyncHandler1.HandledEvents, (e) => e is TestEvent2);
                Assert.Contains(asyncHandler1.HandledEvents, (e) => e is TestEvent3);

                // AsyncHandler2 should have 3 events.
                // 1. TestEvent1
                // 2. TestEvent2
                // 3. TestEvent3
                Assert.Equal(3, asyncHandler2.HandledEvents.Count);
                Assert.Contains(asyncHandler2.HandledEvents, (e) => e is TestEvent1);
                Assert.Contains(asyncHandler2.HandledEvents, (e) => e is TestEvent2);
                Assert.Contains(asyncHandler2.HandledEvents, (e) => e is TestEvent3);

                // AsyncHandler3 should have 3 events.
                // 1. TestEvent1
                // 2. TestEvent2
                // 3. TestEvent3
                Assert.Equal(3, asyncHandler3.HandledEvents.Count);
                Assert.Contains(asyncHandler3.HandledEvents, (e) => e is TestEvent1);
                Assert.Contains(asyncHandler3.HandledEvents, (e) => e is TestEvent2);
                Assert.Contains(asyncHandler3.HandledEvents, (e) => e is TestEvent3);

                // Handler1 should have 3 events.
                // 1. TestEvent1
                // 2. TestEvent2
                // 3. TestEvent3
                Assert.Equal(3, handler1.HandledEvents.Count);
                Assert.Contains(handler1.HandledEvents, (e) => e is TestEvent1);
                Assert.Contains(handler1.HandledEvents, (e) => e is TestEvent2);
                Assert.Contains(handler1.HandledEvents, (e) => e is TestEvent3);

                // Handler2 should have 3 events.
                // 1. TestEvent1
                // 2. TestEvent2
                // 3. TestEvent3
                Assert.Equal(3, handler2.HandledEvents.Count);
                Assert.Contains(handler2.HandledEvents, (e) => e is TestEvent1);
                Assert.Contains(handler2.HandledEvents, (e) => e is TestEvent2);
                Assert.Contains(handler2.HandledEvents, (e) => e is TestEvent3);

                // Handler3 should have 3 events.
                // 1. TestEvent1
                // 2. TestEvent2
                // 3. TestEvent3
                Assert.Equal(3, handler3.HandledEvents.Count);
                Assert.Contains(handler3.HandledEvents, (e) => e is TestEvent1);
                Assert.Contains(handler3.HandledEvents, (e) => e is TestEvent2);
                Assert.Contains(handler3.HandledEvents, (e) => e is TestEvent3);
            }
            
            [Fact]
            public async Task Should_Trigger_OnError_If_EventHandlerRegistration_Produces_Null_Instance()
            {
                var registration = new EventHandlerRegistration();
                // Produces null.
                registration.Register(() => (IEventAsyncHandler<TestEvent1>)null);

                var publisher = new EventPublisher(registration);

                publisher.OnError += (@event, ex) =>
                {
                    _testOutputHelper.WriteLine(ex.Message);

                    Assert.IsType<InvalidOperationException>(ex);
                };

                await publisher.PublishAsync(new TestEvent1());
            }

            #endregion Publish To Registered Event Handlers

            #region Publish To Container Resolved Event Handlers

            [Fact]
            public async Task Should_Invoke_All_Registered_Event_Handlers_In_Container()
            {
                var container = new Container();
                container.RegisterCollection(typeof(IEventAsyncHandler<>), typeof(TestEvent1).Assembly);
                container.RegisterCollection(typeof(IEventHandler<>), typeof(TestEvent1).Assembly);
                container.Register(() => _testOutputHelper);

                var adapter = new SimpleInjectorContainerAdapter(container);
                var eventHandlerResolver = new ContainerEventHandlerResolver(adapter);
                var publisher = new EventPublisher(eventHandlerResolver);

                publisher.OnError += (@event, ex) =>
                {
                    _testOutputHelper.WriteLine(ex.Message);

                    Assert.IsType<TestEventHandlerException>(ex);
                };

                await publisher.PublishAsync(new TestEvent1());
            }

            [Fact]
            public async Task Should_Invoke_All_Registered_Event_Handlers_In_Container_For_Each_Events()
            {
                var container = new Container();
                container.RegisterCollection(typeof(IEventAsyncHandler<>), typeof(TestEvent1).Assembly);
                container.RegisterCollection(typeof(IEventHandler<>), typeof(TestEvent1).Assembly);
                container.Register(() => _testOutputHelper);

                var adapter = new SimpleInjectorContainerAdapter(container);
                var eventHandlerResolver = new ContainerEventHandlerResolver(adapter);
                var publisher = new EventPublisher(eventHandlerResolver);

                publisher.OnError += (@event, ex) =>
                {
                    _testOutputHelper.WriteLine(ex.Message);
                };

                await publisher.PublishAsync(new List<IEvent> { new TestEvent1(), new TestEvent2(), new TestEvent3() });

                // Event may not have yet been handled in background.
                await Task.Delay(500);
            }

            [Fact]
            public async Task Should_Be_Good_If_No_Event_Handler_Is_Registered_In_Container()
            {
                var container = new Container();
                var adapter = new SimpleInjectorContainerAdapter(container);
                var eventHandlerResolver = new ContainerEventHandlerResolver(adapter);
                var publisher = new EventPublisher(eventHandlerResolver);

                publisher.OnError += (@event, ex) =>
                {
                    _testOutputHelper.WriteLine(ex.Message);
                };

                await publisher.PublishAsync(new TestEvent1());
            }

            #endregion Publish To Container Resolved Event Handlers

            #region Publish To Attributed Event Handlers

            [Fact]
            public async Task Should_Invoke_All_Registered_Event_Handlers_In_Attribute_Registration()
            {
                var attributedHandler1 = new TestAttributedEventHandler1(_testOutputHelper);
                var attributedHandler2 = new TestAttributedEventHandler2(_testOutputHelper);
                var attributedHandler3 = new TestAttributedEventHandler3(_testOutputHelper);

                var registration = new EventHandlerAttributeRegistration();
                registration.Register(() => attributedHandler1);
                registration.Register(() => attributedHandler2);
                registration.Register(() => attributedHandler3);

                var publisher = new EventPublisher(registration);

                publisher.OnError += (eventHandler, ex) =>
                {
                    _testOutputHelper.WriteLine(ex.Message);
                };

                await publisher.PublishAsync(new TestEvent1());

                // Handler1 should have 2 events.
                // It contains sync and async handlers for:
                // 1. TestEvent
                Assert.Equal(2, attributedHandler1.HandledEvents.Count);
                Assert.Contains(attributedHandler1.HandledEvents, e => e is TestEvent1);

                // Handler2 should have 2 events.
                // It contains sync and async handlers for:
                // 1. TestEvent
                Assert.Equal(2, attributedHandler2.HandledEvents.Count);
                Assert.Contains(attributedHandler2.HandledEvents, e => e is TestEvent1);

                // Handler3 should have 2 events.
                // It contains sync and async handlers for:
                // 1. TestEvent
                Assert.Equal(2, attributedHandler3.HandledEvents.Count);
                Assert.Contains(attributedHandler3.HandledEvents, e => e is TestEvent1);
            }

            [Fact]
            public async Task Should_Invoke_All_Registered_Event_Handlers_In_Attribute_Registration_For_Each_Events()
            {
                var attributedHandler1 = new TestAttributedEventHandler1(_testOutputHelper);
                var attributedHandler2 = new TestAttributedEventHandler2(_testOutputHelper);
                var attributedHandler3 = new TestAttributedEventHandler3(_testOutputHelper);

                var registration = new EventHandlerAttributeRegistration();
                registration.Register(() => attributedHandler1);
                registration.Register(() => attributedHandler2);
                registration.Register(() => attributedHandler3);

                var publisher = new EventPublisher(registration);

                publisher.OnError += (@event, ex) =>
                {
                    _testOutputHelper.WriteLine(ex.Message);
                };

                await publisher.PublishAsync(new List<IEvent> { new TestEvent1(), new TestEvent2(), new TestEvent3() });
            }

            [Fact]
            public async Task Should_Trigger_OnError_If_EventHandlerAttributeRegistration_Produces_Null_Instance()
            {
                var registration = new EventHandlerAttributeRegistration();
                // Produces null.
                registration.Register<TestAttributedEventHandler1>(() => null);

                var publisher = new EventPublisher(registration);

                publisher.OnError += (eventHandler, ex) =>
                {
                    _testOutputHelper.WriteLine(ex.Message);

                    Assert.IsType<InvalidOperationException>(ex);
                };

                await publisher.PublishAsync(new TestEvent1());
            }

            #endregion Publish to Attributed Event Handlers

            #region Cancellation

            [Fact]
            public Task Should_Throw_If_Cancellation_Token_Source_Is_Cancelled()
            {
                return Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                {
                    var asyncHandler1 = new TestEventAsyncHandler1(_testOutputHelper);

                    var registration = new EventHandlerRegistration();
                    registration.Register<TriggerLongRunningEvent>(() => asyncHandler1);

                    var publisher = new EventPublisher(registration);

                    publisher.OnError += (@event, ex) =>
                    {
                        _testOutputHelper.WriteLine(ex.Message);
                    };

                    var cts = new CancellationTokenSource();

                    Task publishTask = publisher.PublishAsync(new TriggerLongRunningEvent(1000), cts.Token);

                    cts.Cancel();

                    try
                    {
                        await publishTask;
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.Message);
                        throw;
                    }
                });
            }

            #endregion Cancellation

            #region Null Check

            [Fact]
            public Task Should_Throw_If_Null_Is_Being_Published()
            {
                return Assert.ThrowsAsync<ArgumentNullException>(async () =>
                {
                    var registration = new EventHandlerRegistration();
                    var publisher = new EventPublisher(registration);
                    try
                    {
                        await publisher.PublishAsync((IEvent)null);
                    }
                    catch(Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.Message);
                        throw;
                    }
                });
            }

            #endregion Null Check
        }

        #endregion PublishAsync Method
    }
}
