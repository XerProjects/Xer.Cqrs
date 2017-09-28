using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.Dispatchers;
using Xer.Cqrs.Registrations.QueryHandlers;
using Xer.Cqrs.Tests.Mocks;
using Xer.Cqrs.Tests.Mocks.CommandHandlers;
using Xer.Cqrs.Tests.Mocks.QueryHandlers;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests
{
    public class QueryDispatcherTests
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
            public async Task Dispatch_Query_To_Registered_Handler()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingAsync, string>)new TestQueryHandler(_outputHelper));

                string data = "Test async message.";

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync(new QuerySomethingAsync(data));

                Assert.Equal(result, data);
            }

            [Fact]
            public async Task Dispatch_Query_Multiple_Times_To_Registered_Handler()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomething, string>)new TestQueryHandler(_outputHelper));
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingNonReferenceType, int>)new TestQueryHandler(_outputHelper));

                string data1 = "Test message 1.";
                string data2 = "Test message 2.";

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.DispatchAsync(new QuerySomething(data1));
                var result2 = dispatcher.DispatchAsync(new QuerySomething(data2));
                var result3 = dispatcher.DispatchAsync(new QuerySomethingNonReferenceType(1));

                await Task.WhenAll(result1, result2, result3);

                Assert.Equal(await result1, data1);
                Assert.Equal(await result2, data2);
                Assert.Equal(await result3, 1);
            }

            [Fact]
            public async Task Dispatch_Query_To_Registered_Handler_With_CancellationToken()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingAsyncWithDelay, string>)new TestQueryHandler(_outputHelper));

                var cts = new CancellationTokenSource();

                var dispatcher = new QueryDispatcher(registration);

                string data = "Test async message with cancellation token.";

                var result = await dispatcher.DispatchAsync(new QuerySomethingAsyncWithDelay(data, 500), cts.Token);

                Assert.Equal(result, data);
            }

            [Fact]
            public void Dispatch_Query_And_Cancel()
            {
                Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                {
                    var registration = new QueryHandlerRegistration();
                    registration.Register(() => (IQueryAsyncHandler<QuerySomethingAsyncWithDelay, string>)new TestQueryHandler(_outputHelper));

                    var cts = new CancellationTokenSource();

                    var dispatcher = new QueryDispatcher(registration);
                    Task task = dispatcher.DispatchAsync(new QuerySomethingAsyncWithDelay("This will be cancelled", 2000), cts.Token);

                    cts.Cancel();

                    await task;
                });
            }

            [Fact]
            public Task Dispatch_Should_Propagate_Exceptions_From_Handlers()
            {
                return Assert.ThrowsAnyAsync<Exception>(() =>
                {
                    try
                    {
                        var registration = new QueryHandlerRegistration();
                        registration.Register(() => (IQueryAsyncHandler<QuerySomethingWithException, string>)new TestQueryHandler(_outputHelper));

                        var dispatcher = new QueryDispatcher(registration);

                        return dispatcher.DispatchAsync(new QuerySomethingWithException("This will cause an exception."));
                    }
                    catch (Exception ex)
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
            public void Dispatch_To_Registered_Query_Handler()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryHandler<QuerySomething, string>)new TestQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result = dispatcher.Dispatch(new QuerySomething("Test message."));

                Assert.Equal(result, "Test message.");
            }

            [Fact]
            public async Task Dispatch_Query_With_Non_Reference_Type_Result()
            {
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryHandler<QuerySomethingNonReferenceType, int>)new TestQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync(new QuerySomethingNonReferenceType(1973));

                Assert.Equal(result, 1973);
            }

            [Fact]
            public void Dispatch_Should_Propagate_Exceptions_From_Handlers()
            {
                Assert.ThrowsAny<Exception>(() =>
                {
                    try
                    {
                        var registration = new QueryHandlerRegistration();
                        registration.Register(() => (IQueryHandler<QuerySomethingWithException, string>)new TestQueryHandler(_outputHelper));

                        var dispatcher = new QueryDispatcher(registration);

                        dispatcher.Dispatch(new QuerySomethingWithException("This will cause an exception."));
                    }
                    catch (Exception ex)
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
