using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xer.Cqrs.EventStack;
using Xer.Cqrs.EventStack.Resolvers;
using Xer.Cqrs.Tests.Entities;
using Xunit;
using Xunit.Abstractions;
using Xer.Delegator.Registrations;
using Xer.Delegator;
using System.Linq;

namespace Xer.Cqrs.Tests.Events
{
    public class EventDelegatorTests
    {
        #region SendAsync Method

        public class SendAsyncMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public SendAsyncMethod(ITestOutputHelper outputHelper)
            {
                _outputHelper = outputHelper;
            }

            #region Basic Registration

            [Fact]
            public async Task Should_Send_Event_To_All_Registered_Event_Handlers()
            {
                var asyncHandler1 = new TestEventHandler(_outputHelper);
                var asyncHandler2 = new TestEventHandler(_outputHelper);
                var asyncHandler3 = new TestEventHandler(_outputHelper);
                var handler1 = new TestEventHandler(_outputHelper);
                var handler2 = new TestEventHandler(_outputHelper);
                var handler3 = new TestEventHandler(_outputHelper);

                var registration = new MultiMessageHandlerRegistration();
                registration.RegisterEventHandler<TestEvent1>(() => handler1.AsEventSyncHandler<TestEvent1>());
                registration.RegisterEventHandler<TestEvent1>(() => handler2.AsEventSyncHandler<TestEvent1>());
                registration.RegisterEventHandler<TestEvent1>(() => handler3.AsEventSyncHandler<TestEvent1>());
                registration.RegisterEventHandler<TestEvent1>(() => asyncHandler1);
                registration.RegisterEventHandler<TestEvent1>(() => asyncHandler2);
                registration.RegisterEventHandler<TestEvent1>(() => asyncHandler3);

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                var delegator = new EventDelegator(resolver);
                await delegator.SendAsync(new TestEvent1());

                // AsyncHandler1 should have 1 event.
                // 1. TestEvent1
                Assert.Equal(1, asyncHandler1.HandledEvents.Count);
                Assert.True(asyncHandler1.HasHandledEvent<TestEvent1>());

                // AsyncHandler2 should have 1 event.
                // 1. TestEvent1
                Assert.Equal(1, asyncHandler2.HandledEvents.Count);
                Assert.True(asyncHandler2.HasHandledEvent<TestEvent1>());

                // AsyncHandler3 should have 1 event.
                // 1. TestEvent1
                Assert.Equal(1, asyncHandler3.HandledEvents.Count);
                Assert.True(asyncHandler3.HasHandledEvent<TestEvent1>());

                // Handler1 should have 1 event.
                // 1. TestEvent1
                Assert.Equal(1, handler1.HandledEvents.Count);
                Assert.True(handler1.HasHandledEvent<TestEvent1>());

                // Handler2 should have 1 event.
                // 1. TestEvent1
                Assert.Equal(1, handler2.HandledEvents.Count);
                Assert.True(handler2.HasHandledEvent<TestEvent1>());

                // Handler3 should have 1 event.
                // 1. TestEvent1
                Assert.Equal(1, handler3.HandledEvents.Count);
                Assert.True(handler3.HasHandledEvent<TestEvent1>());
            }

            [Fact]
            public async Task Should_Send_Each_Events_To_Registered_Event_Handlers()
            {
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

                registration.RegisterEventHandler<TestEvent2>(() => asyncHandler1);
                registration.RegisterEventHandler<TestEvent2>(() => asyncHandler2);
                registration.RegisterEventHandler<TestEvent2>(() => asyncHandler3);
                registration.RegisterEventHandler<TestEvent2>(() => handler1.AsEventSyncHandler<TestEvent2>());
                registration.RegisterEventHandler<TestEvent2>(() => handler2.AsEventSyncHandler<TestEvent2>());
                registration.RegisterEventHandler<TestEvent2>(() => handler3.AsEventSyncHandler<TestEvent2>());

                registration.RegisterEventHandler<TestEvent3>(() => asyncHandler1);
                registration.RegisterEventHandler<TestEvent3>(() => asyncHandler2);
                registration.RegisterEventHandler<TestEvent3>(() => asyncHandler3);
                registration.RegisterEventHandler<TestEvent3>(() => handler1.AsEventSyncHandler<TestEvent3>());
                registration.RegisterEventHandler<TestEvent3>(() => handler2.AsEventSyncHandler<TestEvent3>());
                registration.RegisterEventHandler<TestEvent3>(() => handler3.AsEventSyncHandler<TestEvent3>());

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                var delegator = new EventDelegator(resolver);

                var events = new List<object> { new TestEvent1(), new TestEvent2(), new TestEvent3() };

                await delegator.SendAllAsync(events);

                // AsyncHandler1 should have 3 events.
                // 1. TestEvent1
                // 2. TestEvent2
                // 3. TestEvent3
                Assert.Equal(3, asyncHandler1.HandledEvents.Count);
                Assert.True(asyncHandler1.HasHandledEvent<TestEvent1>());
                Assert.True(asyncHandler1.HasHandledEvent<TestEvent2>());
                Assert.True(asyncHandler1.HasHandledEvent<TestEvent3>());

                // AsyncHandler2 should have 3 events.
                // 1. TestEvent1
                // 2. TestEvent2
                // 3. TestEvent3
                Assert.Equal(3, asyncHandler2.HandledEvents.Count);
                Assert.True(asyncHandler2.HasHandledEvent<TestEvent1>());
                Assert.True(asyncHandler2.HasHandledEvent<TestEvent2>());
                Assert.True(asyncHandler2.HasHandledEvent<TestEvent3>());

                // AsyncHandler3 should have 3 events.
                // 1. TestEvent1
                // 2. TestEvent2
                // 3. TestEvent3
                Assert.Equal(3, asyncHandler3.HandledEvents.Count);
                Assert.True(asyncHandler3.HasHandledEvent<TestEvent1>());
                Assert.True(asyncHandler3.HasHandledEvent<TestEvent2>());
                Assert.True(asyncHandler3.HasHandledEvent<TestEvent3>());

                // Handler1 should have 3 events.
                // 1. TestEvent1
                // 2. TestEvent2
                // 3. TestEvent3
                Assert.Equal(3, handler1.HandledEvents.Count);
                Assert.True(handler1.HasHandledEvent<TestEvent1>());
                Assert.True(handler1.HasHandledEvent<TestEvent2>());
                Assert.True(handler1.HasHandledEvent<TestEvent3>());

                // Handler2 should have 3 events.
                // 1. TestEvent1
                // 2. TestEvent2
                // 3. TestEvent3
                Assert.Equal(3, handler2.HandledEvents.Count);
                Assert.True(handler2.HasHandledEvent<TestEvent1>());
                Assert.True(handler2.HasHandledEvent<TestEvent2>());
                Assert.True(handler2.HasHandledEvent<TestEvent3>());

                // Handler3 should have 3 events.
                // 1. TestEvent1
                // 2. TestEvent2
                // 3. TestEvent3
                Assert.Equal(3, handler3.HandledEvents.Count);
                Assert.True(handler3.HasHandledEvent<TestEvent1>());
                Assert.True(handler3.HasHandledEvent<TestEvent2>());
                Assert.True(handler3.HasHandledEvent<TestEvent3>());
            }
            
            [Fact]
            public Task Should_Throw_If_Event_Handler_Factory_Produces_Null_Instance()
            {
                return Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    var registration = new MultiMessageHandlerRegistration();
                    // Produces null.
                    registration.RegisterEventHandler<TestEvent1>(() => null);

                    var delegator = new EventDelegator(registration.BuildMessageHandlerResolver());

                    try
                    {
                        await delegator.SendAsync(new TestEvent1());
                    }
                    catch(Exception ex)
                    {
                        _outputHelper.WriteLine(ex.Message);
                        throw;
                    }
                });
            }

            #endregion Basic Registration

            #region Attribute Registration

            [Fact]
            public async Task Should_Send_Event_To_Registered_Attributed_Event_Handlers()
            {
                var attributedHandler1 = new TestAttributedEventHandler(_outputHelper);
                var attributedHandler2 = new TestAttributedEventHandler(_outputHelper);
                var attributedHandler3 = new TestAttributedEventHandler(_outputHelper);

                var registration = new MultiMessageHandlerRegistration();
                registration.RegisterEventHandlerAttributes(() => attributedHandler1);
                registration.RegisterEventHandlerAttributes(() => attributedHandler2);
                registration.RegisterEventHandlerAttributes(() => attributedHandler3);

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                var delegator = new EventDelegator(resolver);

                await delegator.SendAsync(new TestEvent1());

                // Handler1 should have 2 events.
                // It contains sync and async handlers for:
                // 1. TestEvent1
                Assert.Equal(2, attributedHandler1.HandledEvents.Count);
                Assert.True(attributedHandler1.HasHandledEvent<TestEvent1>());

                // Handler2 should have 2 events.
                // It contains sync and async handlers for:
                // 1. TestEvent1
                Assert.Equal(2, attributedHandler2.HandledEvents.Count);
                Assert.True(attributedHandler2.HasHandledEvent<TestEvent1>());

                // Handler3 should have 2 events.
                // It contains sync and async handlers for:
                // 1. TestEvent1
                Assert.Equal(2, attributedHandler3.HandledEvents.Count);
                Assert.True(attributedHandler3.HasHandledEvent<TestEvent1>());
            }

            [Fact]
            public async Task Should_Send_Each_Event_To_All_Registered_Attributed_Event_Handlers()
            {
                var attributedHandler1 = new TestAttributedEventHandler(_outputHelper);
                var attributedHandler2 = new TestAttributedEventHandler(_outputHelper);
                var attributedHandler3 = new TestAttributedEventHandler(_outputHelper);

                var registration = new MultiMessageHandlerRegistration();
                registration.RegisterEventHandlerAttributes(() => attributedHandler1);
                registration.RegisterEventHandlerAttributes(() => attributedHandler2);
                registration.RegisterEventHandlerAttributes(() => attributedHandler3);

                IMessageHandlerResolver resolver = registration.BuildMessageHandlerResolver();

                var delegator = new EventDelegator(resolver);

                var events = new List<object> { new TestEvent1(), new TestEvent2(), new TestEvent3() };
                await delegator.SendAllAsync(events);

                Assert.True(attributedHandler1.HasHandledEvent<TestEvent1>());
                Assert.True(attributedHandler1.HasHandledEvent<TestEvent2>());
                Assert.True(attributedHandler1.HasHandledEvent<TestEvent3>());

                Assert.True(attributedHandler2.HasHandledEvent<TestEvent1>());
                Assert.True(attributedHandler2.HasHandledEvent<TestEvent2>());
                Assert.True(attributedHandler2.HasHandledEvent<TestEvent3>());

                Assert.True(attributedHandler3.HasHandledEvent<TestEvent1>());
                Assert.True(attributedHandler3.HasHandledEvent<TestEvent2>());
                Assert.True(attributedHandler3.HasHandledEvent<TestEvent3>());
            }

            [Fact]
            public Task Should_Throw_If_Instance_Factory_Produces_Null()
            {
                return Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    var registration = new MultiMessageHandlerRegistration();
                    // Produces null.
                    registration.RegisterEventHandlerAttributes<TestAttributedEventHandler>(() => null);

                    var delegator = new EventDelegator(registration.BuildMessageHandlerResolver());

                    try
                    {
                        await delegator.SendAsync(new TestEvent1());
                    }
                    catch(Exception ex)
                    {
                        _outputHelper.WriteLine(ex.Message);
                        throw;
                    }
                });
            }

            [Fact]
            public Task Should_Throw_If_Registration_Instance_Factory_Produces_Null()
            {
                return Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    var commandHandler = new TestAttributedEventHandler(_outputHelper);

                    var registration = new MultiMessageHandlerRegistration();
                    registration.RegisterEventHandlerAttributes(EventHandlerAttributeRegistration.ForType<TestAttributedEventHandler>(() => null));

                    try
                    {
                        var delegator = new EventDelegator(registration.BuildMessageHandlerResolver());
                        await delegator.SendAsync(new TestEvent1());
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            [Fact]
            public Task Should_Throw_If_Instance_From_Factory_Does_Not_Match_Registration_Type()
            {
                return Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    var commandHandler = new TestAttributedEventHandler(_outputHelper);

                    var registration = new MultiMessageHandlerRegistration();
                    registration.RegisterEventHandlerAttributes(EventHandlerAttributeRegistration.ForType(typeof(TestAttributedEventHandler), 
                                                                                                          () => new TestCommandHandler(_outputHelper)));

                    try
                    {
                        var delegator = new EventDelegator(registration.BuildMessageHandlerResolver());
                        await delegator.SendAsync(new TestEvent1());
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            [Fact]
            public Task Should_Propagate_If_Registration_Instance_Factory_Throws_An_Exception()
            {
                return Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    var commandHandler = new TestAttributedEventHandler(_outputHelper);

                    var registration = new MultiMessageHandlerRegistration();
                    registration.RegisterEventHandlerAttributes(EventHandlerAttributeRegistration.ForType<TestAttributedEventHandler>(() => throw new Exception("Intentional exception.")));

                    try
                    {
                        var delegator = new EventDelegator(registration.BuildMessageHandlerResolver());
                        await delegator.SendAsync(new TestEvent1());
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            #endregion Attribute Registration

            #region Container Registration

            [Fact]
            public async Task Should_Send_Event_To_All_Registered_Event_Handlers_In_Container()
            {
                var container = new Container();
                container.RegisterCollection(typeof(IEventAsyncHandler<>), typeof(TestEvent1).Assembly);
                container.RegisterCollection(typeof(IEventHandler<>), typeof(TestEvent1).Assembly);
                container.Register(() => _outputHelper);

                var adapter = new SimpleInjectorContainerAdapter(container);
                var eventHandlerResolver = new ContainerEventHandlerResolver(adapter, 
                                                                             yieldExecutionOfSyncHandlers: true, 
                                                                             resolveExceptionHandler: (ex) => _outputHelper.WriteLine(ex.Message));
                var publisher = new MessageDelegator(eventHandlerResolver);

                await publisher.SendAsync(new TestEvent1());
            }

            [Fact]
            public async Task Should_Send_Each_Event_To_All_Registered_Event_Handlers_In_Container()
            {
                var containerWithEventHandlers = new Container();
                containerWithEventHandlers.RegisterCollection(typeof(IEventAsyncHandler<>), typeof(TestEvent1).Assembly);
                containerWithEventHandlers.RegisterCollection(typeof(IEventHandler<>), typeof(TestEvent1).Assembly);
                containerWithEventHandlers.Register(() => _outputHelper);

                var container = new Container();
                // Register event handlers as singletons in this container so that we can check HandledEvents property.
                container.RegisterCollection(typeof(IEventAsyncHandler<TestEvent1>), containerWithEventHandlers.GetAllInstances<IEventAsyncHandler<TestEvent1>>().ToList());
                container.RegisterCollection(typeof(IEventAsyncHandler<TestEvent2>), containerWithEventHandlers.GetAllInstances<IEventAsyncHandler<TestEvent2>>().ToList());
                container.RegisterCollection(typeof(IEventAsyncHandler<TestEvent3>), containerWithEventHandlers.GetAllInstances<IEventAsyncHandler<TestEvent3>>().ToList());
                container.RegisterCollection(typeof(IEventHandler<TestEvent1>), containerWithEventHandlers.GetAllInstances<IEventHandler<TestEvent1>>().ToList());
                container.RegisterCollection(typeof(IEventHandler<TestEvent2>), containerWithEventHandlers.GetAllInstances<IEventHandler<TestEvent2>>().ToList());
                container.RegisterCollection(typeof(IEventHandler<TestEvent3>), containerWithEventHandlers.GetAllInstances<IEventHandler<TestEvent3>>().ToList());
                container.Register(() => _outputHelper);

                var adapter = new SimpleInjectorContainerAdapter(container);
                var eventHandlerResolver = new ContainerEventHandlerResolver(adapter, 
                                                                             yieldExecutionOfSyncHandlers: true, 
                                                                             resolveExceptionHandler: (ex) => _outputHelper.WriteLine(ex.Message));
                var delegator = new EventDelegator(eventHandlerResolver);

                var events = new List<object> { new TestEvent1(), new TestEvent2(), new TestEvent3() };
                await delegator.SendAllAsync(events);

                // TestEvent1 Async Handler
                var testEvent1AsyncHandlers = container.GetAllInstances<IEventAsyncHandler<TestEvent1>>()
                                                        .Cast<TestEventHandler>()
                                                        .ToList();
                Assert.Equal(testEvent1AsyncHandlers.Count, testEvent1AsyncHandlers.Sum(h => h.HandledEvents.Count));
                Assert.True(testEvent1AsyncHandlers.All(h => h.HasHandledEvent<TestEvent1>()));

                // TestEvent2 Async Handler
                var testEvent2AsyncHandlers = container.GetAllInstances<IEventAsyncHandler<TestEvent2>>()
                                                        .Cast<TestEventHandler>()
                                                        .ToList();
                Assert.Equal(testEvent2AsyncHandlers.Count, testEvent2AsyncHandlers.Sum(h => h.HandledEvents.Count));
                Assert.True(testEvent2AsyncHandlers.All(h => h.HasHandledEvent<TestEvent2>()));
                
                // TestEvent3 Async Handler
                var testEvent3AsyncHandlers = container.GetAllInstances<IEventAsyncHandler<TestEvent3>>()
                                                        .Cast<TestEventHandler>()
                                                        .ToList();
                Assert.Equal(testEvent3AsyncHandlers.Count, testEvent3AsyncHandlers.Sum(h => h.HandledEvents.Count));
                Assert.True(testEvent3AsyncHandlers.All(h => h.HasHandledEvent<TestEvent3>()));

                // TestEvent1 Handler
                var testEvent1Handlers = container.GetAllInstances<IEventHandler<TestEvent1>>()
                                                    .Cast<TestEventHandler>()
                                                    .ToList();
                Assert.Equal(testEvent1Handlers.Count, testEvent1Handlers.Sum(h => h.HandledEvents.Count));
                Assert.True(testEvent1Handlers.All(h => h.HasHandledEvent<TestEvent1>()));

                // TestEvent2 Handler
                var testEvent2Handlers = container.GetAllInstances<IEventHandler<TestEvent2>>()
                                                    .Cast<TestEventHandler>()
                                                    .ToList();
                Assert.Equal(testEvent2Handlers.Count, testEvent2Handlers.Sum(h => h.HandledEvents.Count));
                Assert.True(testEvent2Handlers.All(h => h.HasHandledEvent<TestEvent2>()));

                // TestEvent3 Handler
                var testEvent3Handlers = container.GetAllInstances<IEventHandler<TestEvent3>>()
                                                    .Cast<TestEventHandler>()
                                                    .ToList();
                Assert.Equal(testEvent3Handlers.Count, testEvent3Handlers.Sum(h => h.HandledEvents.Count));
                Assert.True(testEvent3Handlers.All(h => h.HasHandledEvent<TestEvent3>()));
            }

            [Fact]
            public async Task Should_Not_Throw_If_No_Event_Handler_Is_Registered_In_Container()
            {
                var container = new Container();
                var adapter = new SimpleInjectorContainerAdapter(container);
                var eventHandlerResolver = new ContainerEventHandlerResolver(adapter);
                var delegator = new EventDelegator(eventHandlerResolver);

                await delegator.SendAsync(new TestEvent1());
            }

            #endregion Container Registration
        }

        #endregion SendAsync Method
    }
}
