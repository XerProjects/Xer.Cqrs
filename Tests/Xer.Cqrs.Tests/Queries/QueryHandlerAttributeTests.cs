using System;
using System.Threading.Tasks;
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
                var result = await dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomethingAsync("Test async message."));

                Assert.Equal("Test async message.", result);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler_With_Cancellation_Token()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomethingAsyncWithDelay("Test async message with cancellation token.", 10000));

                Assert.Equal("Test async message with cancellation token.", result);
            }

            [Fact]
            public async Task Should_Invoke_Registered_Query_Handler_When_Dispatched_Multiple_Times()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething("Test message 1."));
                var result2 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething("Test message 2."));
                var result3 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething("Test message 3."));

                await Task.WhenAll(result1, result2, result3);

                Assert.Equal("Test message 1.", await result1);
                Assert.Equal("Test message 2.", await result2);
                Assert.Equal("Test message 3.", await result3);
            }

            [Fact]
            public void Should_Propagate_Exception_From_Query_Handler()
            {
                Assert.ThrowsAnyAsync<Exception>(async () =>
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
                var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomethingAsync("Test async message."));

                Assert.Equal("Test async message.", result);
            }

            [Fact]
            public void Should_Invoke_Registered_Query_Handler_With_Cancellation_Token()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomethingAsyncWithDelay("Test async message with cancellation token.", 10000));

                Assert.Equal("Test async message with cancellation token.", result);
            }

            [Fact]
            public void Should_Invoke_Registered_Query_Handler_When_Dispatched_Multiple_Times()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.Register(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething("Test message 1."));
                var result2 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething("Test message 2."));
                var result3 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething("Test message 3."));
            }

            [Fact]
            public void Should_Propagate_Exception_From_Query_Handler()
            {
                Assert.ThrowsAny<Exception>(() =>
                {
                    try
                    {
                        var registration = new QueryHandlerAttributeRegistration();
                        registration.Register(() => new TestAttributedQueryHandler(_outputHelper));

                        var dispatcher = new QueryDispatcher(registration);

                        dispatcher.Dispatch<QuerySomethingWithException, string>(new QuerySomethingWithException("This will cause an exception."));
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
