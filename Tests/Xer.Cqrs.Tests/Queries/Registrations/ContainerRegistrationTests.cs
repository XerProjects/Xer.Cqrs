using SimpleInjector;
using System.Threading.Tasks;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Resolvers;
using Xer.Cqrs.Tests.Mocks;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Queries.Registrations
{
    public class ContainerRegistrationTests
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
            public async Task Should_Store_Registered_Query_Handler_In_Container()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);

                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomethingAsync, string>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryHandlerResolver(containerAdapter);

                const string data = nameof(Should_Store_Registered_Query_Handler_In_Container);

                QueryHandlerDelegate<string> queryHandlerDelegate = resolver.ResolveQueryHandler<QuerySomethingAsync, string>();

                Assert.NotNull(queryHandlerDelegate);

                var registeredQueryHandler = container.GetInstance<IQueryAsyncHandler<QuerySomethingAsync, string>>();

                var query = new QuerySomethingAsync(data);

                var result1 = await queryHandlerDelegate.Invoke(query);
                var result2 = await registeredQueryHandler.HandleAsync(query);

                Assert.Equal(data, result1);
                Assert.Equal(data, result2);
                Assert.Equal(result1, result2);
            }
        }

        #endregion Register Method
    }
}
