using System.Threading.Tasks;
using Xer.Cqrs.AttributeHandlers.Registrations;
using Xer.Cqrs.Dispatchers;
using Xer.Cqrs.Tests.Mocks;
using Xer.Cqrs.Tests.Mocks.QueryHandlers;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.AttributeHandling
{
    public class QueryHandlerAttributeTests
    {
        public class RegisterAttributedMethodsMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public RegisterAttributedMethodsMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public async Task Multiple_DispatchAsync_To_Query_Handler_Attributed_Object()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.RegisterAttributedMethods(() => new TestAttributedQueryHandler(_testOutputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result1 = dispatcher.DispatchAsync(new QuerySomething("Test message 1."));
                var result2 = dispatcher.DispatchAsync(new QuerySomething("Test message 2."));
                var result3 = dispatcher.DispatchAsync(new QuerySomething("Test message 3."));

                await Task.WhenAll(result1, result2, result3);

                Assert.Equal(await result1, "Test message 1.");
                Assert.Equal(await result2, "Test message 2.");
                Assert.Equal(await result3, "Test message 3.");
            }

            [Fact]
            public async Task DispatchAsync_To_Query_Handler_Attributed_Object()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.RegisterAttributedMethods(() => new TestAttributedQueryHandler(_testOutputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync(new QuerySomethingAsync("Test async message."));

                Assert.Equal(result, "Test async message.");
            }

            [Fact]
            public async Task DispatchAsync_To_Query_Handler_Attributed_Object_With_CancellationToken()
            {
                var registration = new QueryHandlerAttributeRegistration();
                registration.RegisterAttributedMethods(() => new TestAttributedQueryHandler(_testOutputHelper));

                var dispatcher = new QueryDispatcher(registration);
                var result = await dispatcher.DispatchAsync(new QuerySomethingAsyncWithDelay("Test async message with cancellation token.", 10000));

                Assert.Equal(result, "Test async message with cancellation token.");
            }
        }
    }
}
