using System.Threading.Tasks;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Registrations;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Queries.Registrations
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
            public async Task Should_Store_All_Query_Handlers()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);
                var registration = new QueryHandlerRegistration();
                registration.Register(() => (IQueryAsyncHandler<QuerySomethingAsync, string>)queryHandler);

                QueryHandlerDelegate<string> queryHandlerDelegate = registration.ResolveQueryHandler<QuerySomethingAsync, string>();

                var query = new QuerySomethingAsync(nameof(Should_Store_All_Query_Handlers));

                var result1 = await queryHandler.HandleAsync(query);

                var result2 = await queryHandlerDelegate.Invoke(query);

                Assert.Equal(result1, result2);
            }
        }

        #endregion Register Method
    }
}
