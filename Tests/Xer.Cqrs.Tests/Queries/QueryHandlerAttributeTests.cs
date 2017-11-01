using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Dispatchers;
using Xer.Cqrs.QueryStack.Registrations;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Queries
{
    public class QueryHandlerAttributeTests
    {
        #region DispatchAsync Method

        public class DispatchAsyncMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public DispatchAsyncMethod(ITestOutputHelper testOutputHelper)
            {
                _outputHelper = testOutputHelper;
            }

            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);

                const string data = nameof(Should_Invoke_Registered_Query_Handler);

                var result = await dispatcher.DispatchAsync<QuerySomethingAsync, string>(new QuerySomethingAsync(data));

                Assert.Equal(data, result);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler_With_Cancellation_Token()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);

                var cancellationToken = new CancellationTokenSource();
                const string data = nameof(Should_Invoke_Registered_Query_Handler_With_Cancellation_Token);

                var result = await dispatcher.DispatchAsync<QuerySomethingAsyncWithDelay, string>(new QuerySomethingAsyncWithDelay(data, 500), cancellationToken.Token);

                Assert.Equal(data, result);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler_When_Dispatched_Multiple_Times()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);

                const string data = nameof(Should_Invoke_Registered_Query_Handler_When_Dispatched_Multiple_Times);
                const string data1 = data + "1";
                const string data2 = data + "2";
                const string data3 = data + "3";

                var result1 = dispatcher.DispatchAsync<QuerySomethingAsync, string>(new QuerySomethingAsync(data1));
                var result2 = dispatcher.DispatchAsync<QuerySomethingAsync, string>(new QuerySomethingAsync(data2));
                var result3 = dispatcher.DispatchAsync<QuerySomethingAsync, string>(new QuerySomethingAsync(data3));

                await Task.WhenAll(result1, result2, result3);

                Assert.Equal(data1, await result1);
                Assert.Equal(data2, await result2);
                Assert.Equal(data3, await result3);
            }

            [Fact]
            public Task Should_Propagate_Exception_From_Query_Handler()
            {
                return Assert.ThrowsAnyAsync<Exception>(async () =>
                {
                    try
                    {
                        var registration = new QueryHandlerAttributeRegistration();
                        registration.Register(() => new TestAttributedQueryHandler(_outputHelper));

                        var dispatcher = new QueryDispatcher(registration);

                        await dispatcher.DispatchAsync<QuerySomethingWithException, string>(new QuerySomethingWithException("This will cause an exception."));
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            [Fact]
            public Task Should_Throw_When_Cancelled()
            {
                return Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                {
                    var registration = new QueryHandlerAttributeRegistration();
                    registration.Register(() => new TestAttributedQueryHandler(_outputHelper));

                    var dispatcher = new QueryDispatcher(registration);

                    var cts = new CancellationTokenSource();
                    const string data = nameof(Should_Throw_When_Cancelled);

                    Task<string> queryTask = dispatcher.DispatchAsync<QuerySomethingAsyncWithDelay, string>(new QuerySomethingAsyncWithDelay(data, 3000), cts.Token);

                    cts.Cancel();

                    try
                    {
                        await queryTask;
                    }
                    catch(Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
            
            [Fact]
            public Task Should_Throw_When_No_Registered_Query_Handler_Is_Found()
            {
                return Assert.ThrowsAsync<QueryNotHandledException>(async () =>
                {
                    var registration = new QueryHandlerAttributeRegistration();
                    var dispatcher = new QueryDispatcher(registration);
                    
                    const string data = nameof(Should_Throw_When_No_Registered_Query_Handler_Is_Found);

                    try
                    {
                        var result = await dispatcher.DispatchAsync<QuerySomethingAsync, string>(new QuerySomethingAsync(data));
                    }
                    catch(Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }

        #endregion DispatchAsync Method

        #region Dispatch Method

        public class DispatchMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public DispatchMethod(ITestOutputHelper testOutputHelper)
            {
                _outputHelper = testOutputHelper;
            }

            [Fact]
            public void Should_Invoke_Registered_Query_Handler()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);

                const string data = nameof(Should_Invoke_Registered_Query_Handler);

                var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomethingAsync(data));

                Assert.Equal(data, result);
            }

            [Fact]
            public void Should_Invoke_Registered_Query_Handler_When_Dispatched_Multiple_Times()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);

                const string data = nameof(Should_Invoke_Registered_Query_Handler_When_Dispatched_Multiple_Times);
                const string data1 = data + "1";
                const string data2 = data + "2";
                const string data3 = data + "3";

                var result1 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data1));
                var result2 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data2));
                var result3 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data3));

                Assert.Equal(data1, result1);
                Assert.Equal(data2, result2);
                Assert.Equal(data3, result3);
            }

            [Fact]
            public void Should_Propagate_Exception_From_Query_Handler()
            {
                Assert.ThrowsAny<Exception>(() =>
                {
                    var registration = new QueryHandlerAttributeRegistration();
                    registration.Register(() => new TestAttributedQueryHandler(_outputHelper));
                    var dispatcher = new QueryDispatcher(registration);

                    try
                    {
                        dispatcher.Dispatch<QuerySomethingWithException, string>(new QuerySomethingWithException("This will cause an exception."));
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }

            [Fact]
            public void Should_Throw_When_No_Registered_Query_Handler_Is_Found()
            {
                Assert.Throws<QueryNotHandledException>(() =>
                {
                    var registration = new QueryHandlerAttributeRegistration();
                    var dispatcher = new QueryDispatcher(registration);

                    const string data = nameof(Should_Throw_When_No_Registered_Query_Handler_Is_Found);

                    try
                    {
                        var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething(data));
                    }
                    catch(Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }

        #endregion Dispatch Method
    }
}
