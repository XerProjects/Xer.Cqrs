using SimpleInjector;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Dispatchers;
using Xer.Cqrs.QueryStack.Registrations;
using Xer.Cqrs.QueryStack.Resolvers;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Queries
{
    public class QueryDispatcherTests
    {
        #region DispatchAsync Method

        public class DispatchAsyncMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public DispatchAsyncMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            #region Basic Registration

            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomething, string>)new TestQueryHandler(_testOutputHelper));

                const string data = nameof(Should_Invoke_Registered_Query_Handler);

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data));

                Assert.Equal(result, data);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler_When_Dispatched_Multiple_Times()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomething, string>)queryHandler);
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingWithNonReferenceTypeResult, int>)new TestQueryHandler(_testOutputHelper));

                const string data1 = "Test message 1.";
                const string data2 = "Test message 2.";
                const int data3 = 1;

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data1));
                var result2 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data2));
                var result3 = dispatcher.DispatchAsync<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(data3));

                await Task.WhenAll(result1, result2, result3);

                Assert.Equal(data1, await result1);
                Assert.Equal(data2, await result2);
                Assert.Equal(data3, await result3);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler_With_Cancellation_Token()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingWithDelay, string>)queryHandler);

                var cts = new CancellationTokenSource();

                var dispatcher = new QueryDispatcher(registration);

                const string data = nameof(Should_Invoke_Registered_Query_Handler_With_Cancellation_Token);

                var result = await dispatcher.DispatchAsync<QuerySomethingWithDelay, string>(new QuerySomethingWithDelay(data, 500), cts.Token);

                Assert.Equal(data, result);
            }

            [Fact]
            public async Task Should_Allow_Registered_Query_Handlers_With_Non_Reference_Type_Query_Results()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryHandler<QuerySomethingWithNonReferenceTypeResult, int>)queryHandler);

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(1973));

                Assert.Equal(1973, result);
            }

            [Fact]
            public Task Should_Throw_When_No_Registered_Query_Handler_Is_Found()
            {
                return Assert.ThrowsAsync<QueryNotHandledException>(async () =>
                {
                    var registration = new QueryHandlerRegistration();
                    var dispatcher = new QueryDispatcher(registration);

                    const string data = nameof(Should_Throw_When_No_Registered_Query_Handler_Is_Found);

                    try
                    {
                        var result = await dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething((string)data));
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            #endregion Basic Registration

            #region Attribute Registration

            [Fact]
            public async Task Should_Invoke_Registered_Attribute_Query_Handler()
            {
                var queryHandler = new TestAttributedQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => queryHandler);

                const string data = nameof(Should_Invoke_Registered_Attribute_Query_Handler);

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data));

                Assert.Equal(result, data);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Attribute_Query_Handler_When_Dispatched_Multiple_Times()
            {
                var queryHandler = new TestAttributedQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => queryHandler);

                const string data1 = "Test message 1.";
                const string data2 = "Test message 2.";
                const int data3 = 1;

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data1));
                var result2 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data2));
                var result3 = dispatcher.DispatchAsync<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(data3));

                await Task.WhenAll(result1, result2, result3);

                Assert.Equal(data1, await result1);
                Assert.Equal(data2, await result2);
                Assert.Equal(data3, await result3);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Attribute_Query_Handler_With_Cancellation_Token()
            {
                var queryHandler = new TestAttributedQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => queryHandler);
                var dispatcher = new QueryDispatcher(registration);

                var cts = new CancellationTokenSource();
                const string data = nameof(Should_Invoke_Registered_Attribute_Query_Handler_With_Cancellation_Token);
                
                var result = await dispatcher.DispatchAsync<QuerySomethingWithDelay, string>(new QuerySomethingWithDelay(data, 500), cts.Token);

                Assert.Equal(data, result);
            }

            [Fact]
            public async Task Should_Allow_Attribute_Query_Handlers_With_Non_Reference_Type_Query_Results()
            {
                var queryHandler = new TestAttributedQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => queryHandler);

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(1973));

                Assert.Equal(1973, result);
            }

            [Fact]
            public Task Should_Throw_When_No_Registered_Attribute_Query_Handler_Is_Found()
            {
                return Assert.ThrowsAsync<QueryNotHandledException>(async () =>
                {
                    var registration = new QueryHandlerAttributeRegistration();
                    var dispatcher = new QueryDispatcher(registration);

                    const string data = nameof(Should_Throw_When_No_Registered_Attribute_Query_Handler_Is_Found);

                    try
                    {
                        var result = await dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething((string)data));
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            #endregion Attribute Registration

            #region Container Registration

            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler_In_Container()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomething, string>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryHandlerResolver(containerAdapter);

                const string data = nameof(Should_Invoke_Registered_Query_Handler_In_Container);

                var dispatcher = new QueryDispatcher(resolver);
                var result = await dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data));

                Assert.Equal(data, result);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler_In_Container_When_Dispatched_Multiple_Times()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomething, string>>(() => queryHandler, Lifestyle.Singleton);
                container.Register<IQueryAsyncHandler<QuerySomethingWithNonReferenceTypeResult, int>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryHandlerResolver(containerAdapter);

                const string data1 = "Test message 1.";
                const string data2 = "Test message 2.";
                const int data3 = 1;

                var dispatcher = new QueryDispatcher(resolver);
                var result1 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data1));
                var result2 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething(data2));
                var result3 = dispatcher.DispatchAsync<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(data3));

                await Task.WhenAll(result1, result2, result3);

                Assert.Equal(data1, await result1);
                Assert.Equal(data2, await result2);
                Assert.Equal(data3, await result3);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler_In_Container_With_Cancellation_Token()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomethingWithDelay, string>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryHandlerResolver(containerAdapter);
                var dispatcher = new QueryDispatcher(resolver);

                var cts = new CancellationTokenSource();
                const string data = nameof(Should_Invoke_Registered_Attribute_Query_Handler_With_Cancellation_Token);

                var result = await dispatcher.DispatchAsync<QuerySomethingWithDelay, string>(new QuerySomethingWithDelay(data, 500), cts.Token);

                Assert.Equal(data, result);
            }

            [Fact]
            public async Task Should_Allow_Registered_Query_Handlers_In_Container_With_Non_Reference_Type_Query_Results()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomethingWithNonReferenceTypeResult, int>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryHandlerResolver(containerAdapter);
                var dispatcher = new QueryDispatcher(resolver);

                var result = await dispatcher.DispatchAsync<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(1973));

                Assert.Equal(1973, result);
            }

            [Fact]
            public Task Should_Throw_When_No_Registered_Query_Handler_In_Container_Is_Found()
            {
                return Assert.ThrowsAsync<QueryNotHandledException>(async () =>
                {
                    var container = new Container();
                    var containerAdapter = new SimpleInjectorContainerAdapter(container);
                    var resolver = new ContainerQueryHandlerResolver(containerAdapter);
                    var dispatcher = new QueryDispatcher(resolver);
                    
                    const string data = nameof(Should_Throw_When_No_Registered_Attribute_Query_Handler_Is_Found);

                    try
                    {
                        var result = await dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething((string)data));
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            #endregion Container Registration

            [Fact]
            public Task Should_Propagate_Exception_From_Query_Handler()
            {
                return Assert.ThrowsAnyAsync<TestQueryHandlerException>(async () =>
                {
                    var queryHandler = new TestQueryHandler(_testOutputHelper);
                    var registration = new QueryHandlerRegistration();
                    registration.Register(() => (IQueryAsyncHandler<QuerySomethingWithException, string>)queryHandler);

                    var dispatcher = new QueryDispatcher(registration);

                    try
                    {
                        await dispatcher.DispatchAsync<QuerySomethingWithException, string>(new QuerySomethingWithException("This will cause an exception."));
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            [Fact]
            public Task Should_Throw_When_Cancelled()
            {
                return Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                {
                    var queryHandler = new TestQueryHandler(_testOutputHelper);
                    var registration = new QueryHandlerRegistration();
                    registration.Register(() => (IQueryAsyncHandler<QuerySomethingWithDelay, string>)queryHandler);

                    var cts = new CancellationTokenSource();

                    var dispatcher = new QueryDispatcher(registration);
                    Task task = dispatcher.DispatchAsync<QuerySomethingWithDelay, string>(new QuerySomethingWithDelay("This will be cancelled", 3000), cts.Token);

                    cts.Cancel();

                    try
                    {
                        await task;
                    }
                    catch(Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }

        #endregion DispatchAsync Method

        #region Dispatch Method

        public class DispatchMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public DispatchMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            #region Basic Registration

            [Fact]
            public void Should_Invoke_Registered_Query_Handler()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomething, string>)queryHandler);

                const string data = nameof(Should_Invoke_Registered_Query_Handler);

                var dispatcher = new QueryDispatcher(registration);
                var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));

                Assert.Equal(result, data);
            }

            [Fact]
            public void Should_Invoke_Registered_Query_Handler_When_Dispatched_Multiple_Times()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomething, string>)queryHandler);
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingWithNonReferenceTypeResult, int>)new TestQueryHandler(_testOutputHelper));

                const string data1 = "Test message 1.";
                const string data2 = "Test message 2.";
                const int data3 = 1;

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data1));
                var result2 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data2));
                var result3 = dispatcher.Dispatch<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(data3));

                Assert.Equal(data1, result1);
                Assert.Equal(data2, result2);
                Assert.Equal(data3, result3);
            }
            
            [Fact]
            public void Should_Allow_Registered_Query_Handlers_With_Non_Reference_Type_Query_Results()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryHandler<QuerySomethingWithNonReferenceTypeResult, int>)queryHandler);

                var dispatcher = new QueryDispatcher(registration);
                var result = dispatcher.Dispatch<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(1973));

                Assert.Equal(1973, result);
            }

            [Fact]
            public void Should_Throw_When_No_Registered_Query_Handler_Is_Found()
            {
                Assert.Throws<QueryNotHandledException>(() =>
                {
                    var registration = new QueryHandlerRegistration();
                    var dispatcher = new QueryDispatcher(registration);

                    const string data = nameof(Should_Throw_When_No_Registered_Query_Handler_Is_Found);

                    try
                    {
                        var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            #endregion Basic Registration

            #region Attribute Registration

            [Fact]
            public void Should_Invoke_Registered_Attribute_Query_Handler()
            {
                var queryHandler = new TestAttributedQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => queryHandler);

                const string data = nameof(Should_Invoke_Registered_Attribute_Query_Handler);

                var dispatcher = new QueryDispatcher(registration);
                var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));

                Assert.Equal(result, data);
            }

            [Fact]
            public void Should_Invoke_Registered_Attribute_Query_Handler_When_Dispatched_Multiple_Times()
            {
                var queryHandler = new TestAttributedQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => queryHandler);

                const string data1 = "Test message 1.";
                const string data2 = "Test message 2.";
                const int data3 = 1;

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data1));
                var result2 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data2));
                var result3 = dispatcher.Dispatch<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(data3));
                
                Assert.Equal(data1, result1);
                Assert.Equal(data2, result2);
                Assert.Equal(data3, result3);
            }

            [Fact]
            public void Should_Allow_Attribute_Query_Handlers_With_Non_Reference_Type_Query_Results()
            {
                var queryHandler = new TestAttributedQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => queryHandler);

                var dispatcher = new QueryDispatcher(registration);
                var result = dispatcher.Dispatch<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(1973));

                Assert.Equal(1973, result);
            }

            [Fact]
            public void Should_Throw_When_No_Registered_Attribute_Query_Handler_Is_Found()
            {
                Assert.Throws<QueryNotHandledException>(() =>
                {
                    var registration = new QueryHandlerAttributeRegistration();
                    var dispatcher = new QueryDispatcher(registration);

                    const string data = nameof(Should_Throw_When_No_Registered_Attribute_Query_Handler_Is_Found);

                    try
                    {
                        var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            #endregion Attribute Registration

            #region Container Registration

            [Fact]
            public void Should_Invoke_Registered_Query_Handler_In_Container()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomething, string>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryHandlerResolver(containerAdapter);

                const string data = nameof(Should_Invoke_Registered_Query_Handler_In_Container);

                var dispatcher = new QueryDispatcher(resolver);
                var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));

                Assert.Equal(data, result);
            }

            [Fact]
            public void Should_Invoke_Registered_Query_Handler_In_Container_When_Dispatched_Multiple_Times()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomething, string>>(() => queryHandler, Lifestyle.Singleton);
                container.Register<IQueryAsyncHandler<QuerySomethingWithNonReferenceTypeResult, int>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryHandlerResolver(containerAdapter);

                const string data1 = "Test message 1.";
                const string data2 = "Test message 2.";
                const int data3 = 1;

                var dispatcher = new QueryDispatcher(resolver);
                var result1 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data1));
                var result2 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data2));
                var result3 = dispatcher.Dispatch<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(data3));
                
                Assert.Equal(data1, result1);
                Assert.Equal(data2, result2);
                Assert.Equal(data3, result3);
            }

            [Fact]
            public void Should_Allow_Registered_Query_Handlers_In_Container_With_Non_Reference_Type_Query_Results()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomethingWithNonReferenceTypeResult, int>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryHandlerResolver(containerAdapter);
                var dispatcher = new QueryDispatcher(resolver);

                var result = dispatcher.Dispatch<QuerySomethingWithNonReferenceTypeResult, int>(new QuerySomethingWithNonReferenceTypeResult(1973));

                Assert.Equal(1973, result);
            }

            [Fact]
            public void Should_Throw_When_No_Registered_Query_Handler_In_Container_Is_Found()
            {
                Assert.Throws<QueryNotHandledException>(() =>
                {
                    var queryHandler = new TestQueryHandler(_testOutputHelper);
                    var container = new Container();
                    var containerAdapter = new SimpleInjectorContainerAdapter(container);
                    var resolver = new ContainerQueryHandlerResolver(containerAdapter);
                    var dispatcher = new QueryDispatcher(resolver);

                    const string data = nameof(Should_Throw_When_No_Registered_Attribute_Query_Handler_Is_Found);

                    try
                    {
                        var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            #endregion Container Registration

            [Fact]
            public void Should_Propagate_Exception_From_Query_Handler()
            {
                Assert.Throws<TestQueryHandlerException>(() =>
                {
                    var registration = new QueryHandlerRegistration();
                    registration.Register(() => (IQueryHandler<QuerySomethingWithException, string>)new TestQueryHandler(_testOutputHelper));

                    var dispatcher = new QueryDispatcher(registration);

                    try
                    {
                        dispatcher.Dispatch<QuerySomethingWithException, string>(new QuerySomethingWithException("This will cause an exception."));
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }

        #endregion Dispatch Method
    }
}
