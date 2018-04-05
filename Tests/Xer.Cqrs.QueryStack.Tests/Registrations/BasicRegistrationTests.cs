using System.Threading.Tasks;
using FluentAssertions;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Registrations;
using Xer.Cqrs.QueryStack.Tests.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.QueryStack.Tests.Queries.Registrations
{
    public class BasicRegistrationTests
    {
        #region Register Method
        
       public class RegisterMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public RegisterMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public async Task ShouldRegisterAllQueryHandlers()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomething, string>)queryHandler);

                QueryHandlerDelegate<string> queryHandlerDelegate = registration.ResolveQueryHandler<QuerySomething, string>();

                var query = new QuerySomething(nameof(ShouldRegisterAllQueryHandlers));

                var result1 = await queryHandler.HandleAsync(query);
                var result2 = await queryHandlerDelegate.Invoke(query);

                result1.Should().Be(result2);
            }
        }

        #endregion Register Method
    }
}
