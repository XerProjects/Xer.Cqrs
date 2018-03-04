using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Registrations;
using Xer.Cqrs.QueryStack.Tests.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.QueryStack.Tests.Queries
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
            public async Task ShouldInvokeTheActualRegisteredQueryHandler()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);

                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryHandler<QuerySomething, string>)queryHandler);

                QueryHandlerDelegate<string> queryHandlerDelegate = registration.ResolveQueryHandler<QuerySomething, string>();

                queryHandlerDelegate.Should().NotBeNull();

                const string data = nameof(ShouldInvokeTheActualRegisteredQueryHandler);

                // Invoke.
                await queryHandlerDelegate.Invoke(new QuerySomething(data));

                queryHandler.HandledQueries.Should().HaveCount(1);
                queryHandler.HasHandledQuery<QuerySomething>().Should().BeTrue();
            }

            [Fact]
            public void ShouldCheckForCorrectQueryType()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryHandler<QuerySomethingWithDelay, string>)queryHandler);

                QueryHandlerDelegate<string> queryHandlerDelegate = registration.ResolveQueryHandler<QuerySomethingWithDelay, string>();

                queryHandlerDelegate.Should().NotBeNull();

                const string data = nameof(ShouldCheckForCorrectQueryType);

                Func<Task> action = async () =>
                {
                    try
                    {
                        await queryHandlerDelegate.Invoke(new QuerySomething(data));
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine(ex.ToString());
                        throw;
                    }
                };

                action.Should().Throw<ArgumentException>();
            }
        }
    }
}
