using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Registrations;
using Xer.Cqrs.Tests.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Queries
{
    public class QueryHandlerDelegateTests
    {
        public class InvokeMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public InvokeMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public async Task Should_Invoke_The_Actual_Registered_Query_Handler()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);

                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryHandler<QuerySomething, string>)queryHandler);

                QueryHandlerDelegate<string> queryHandlerDelegate = registration.ResolveQueryHandler<QuerySomething, string>();

                Assert.NotNull(queryHandlerDelegate);

                const string data = nameof(Should_Invoke_The_Actual_Registered_Query_Handler);

                // Invoke.
                await queryHandlerDelegate.Invoke(new QuerySomething(data));

                // Check if actual command handler instance was invoked.
                Assert.Equal(1, queryHandler.HandledQueries.Count);
                Assert.Contains(queryHandler.HandledQueries, c => c is QuerySomething);
            }

            [Fact]
            public Task Should_Check_For_Correct_Query_Type()
            {
                return Assert.ThrowsAnyAsync<ArgumentException>(async () =>
                {
                    var queryHandler = new TestQueryHandler(_testOutputHelper);
                    var registration = new QueryHandlerRegistration();
                    registration.Register(() => (IQueryHandler<QuerySomethingWithDelay, string>)queryHandler);

                    QueryHandlerDelegate<string> queryHandlerDelegate = registration.ResolveQueryHandler<QuerySomethingWithDelay, string>();

                    Assert.NotNull(queryHandlerDelegate);

                    const string data = nameof(Should_Check_For_Correct_Query_Type);

                    try
                    {
                        await queryHandlerDelegate.Invoke(new QuerySomething(data));
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
