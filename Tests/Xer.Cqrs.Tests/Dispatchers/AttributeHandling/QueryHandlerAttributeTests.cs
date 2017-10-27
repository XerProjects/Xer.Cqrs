using System;
using System.Threading.Tasks;
using Xer.Cqrs.QueryStack.Dispatchers;
using Xer.Cqrs.QueryStack.Registrations;
using Xer.Cqrs.Tests.Mocks;
using Xer.Cqrs.Tests.Mocks.QueryHandlers;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.AttributeHandling
{
    public class QueryHandlerAttributeTests
    {
        #region Register Method

        public class RegisterMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public RegisterMethod(ITestOutputHelper testOutputHelper)
            {
                _outputHelper = testOutputHelper;
            }

            [Fact]
            public void Do_Not_Allow_Command_Handlers_With_Void_Return_Type()
            {
                Assert.Throws<InvalidOperationException>(() =>
                {
                    try
                    {
                        var registration = new QueryHandlerAttributeRegistration();
                        registration.RegisterQueryHandlerAttributes(() => new TestAttributedQueryHandlerNoReturnType(_outputHelper));
                    }
                    catch (Exception ex)
                    {
                        _outputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                });
            }
        }

        #endregion Register Method

        #region DispatchAsync Method

        public class DispatchAsyncMethod
        {
            private readonly ITestOutputHelper _outputHelper;

            public DispatchAsyncMethod(ITestOutputHelper testOutputHelper)
            {
                _outputHelper = testOutputHelper;
            }

            [Fact]
            public async Task Multiple_Dispatch_To_Query_Handler_Attributed_Object()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.RegisterQueryHandlerAttributes(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething("Test message 1."));
                var result2 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething("Test message 2."));
                var result3 = dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomething("Test message 3."));

                await Task.WhenAll(result1, result2, result3);

                Assert.Equal(await result1, "Test message 1.");
                Assert.Equal(await result2, "Test message 2.");
                Assert.Equal(await result3, "Test message 3.");
            }

            [Fact]
            public async Task Dispatch_To_Query_Handler_Attributed_Object()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.RegisterQueryHandlerAttributes(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomethingAsync("Test async message."));

                Assert.Equal(result, "Test async message.");
            }

            [Fact]
            public async Task Dispatch_To_Query_Handler_Attributed_Object_With_CancellationToken()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.RegisterQueryHandlerAttributes(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync<QuerySomething, string>(new QuerySomethingAsyncWithDelay("Test async message with cancellation token.", 10000));

                Assert.Equal(result, "Test async message with cancellation token.");
            }

            [Fact]
            public Task Dispatch_Should_Propagate_Exceptions_From_Handlers()
            {
                return Assert.ThrowsAnyAsync<Exception>(() =>
                {
                    try
                    {
                        var registration = new QueryHandlerAttributeRegistration();
                        registration.RegisterQueryHandlerAttributes(() => new TestAttributedQueryHandler(_outputHelper));

                        var dispatcher = new QueryDispatcher(registration);

                        return dispatcher.DispatchAsync<QuerySomethingWithException, string>(new QuerySomethingWithException("This will cause an exception."));
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
            public void Multiple_Dispatch_To_Query_Handler_Attributed_Object()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.RegisterQueryHandlerAttributes(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething("Test message 1."));
                var result2 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething("Test message 2."));
                var result3 = dispatcher.Dispatch<QuerySomething, string>(new QuerySomething("Test message 3."));
            }

            [Fact]
            public void Dispatch_To_Query_Handler_Attributed_Object()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.RegisterQueryHandlerAttributes(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomethingAsync("Test async message."));

                Assert.Equal(result, "Test async message.");
            }

            [Fact]
            public void Dispatch_To_Query_Handler_Attributed_Object_With_CancellationToken()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.RegisterQueryHandlerAttributes(() => new TestAttributedQueryHandler(_outputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result = dispatcher.Dispatch<QuerySomething, string>(new QuerySomethingAsyncWithDelay("Test async message with cancellation token.", 10000));

                Assert.Equal(result, "Test async message with cancellation token.");
            }

            [Fact]
            public void Dispatch_Should_Propagate_Exceptions_From_Handlers()
            {
                Assert.ThrowsAny<Exception>(() =>
                {
                    try
                    {
                        var registration = new QueryHandlerAttributeRegistration();
                        registration.RegisterQueryHandlerAttributes(() => new TestAttributedQueryHandler(_outputHelper));

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
