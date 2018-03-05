using FluentAssertions;
using SimpleInjector;
using System.Threading.Tasks;
using Xer.Cqrs.QueryStack;
using Xer.Cqrs.QueryStack.Resolvers;
using Xer.Cqrs.QueryStack.Tests.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Xer.Cqrs.QueryStack.Tests.Queries.Registrations
{
    public class ContainerResolverTests
    {
        #region Register Method

        public class ResolveQueryHandlerMethod
        {
            private readonly ITestOutputHelper _testOutputHelper;

            public ResolveQueryHandlerMethod(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            [Fact]
            public async Task ShouldResolveQueryHandlersFromContainer()
            {
                var queryHandler = new TestQueryHandler(_testOutputHelper);

                var container = new Container();
                container.Register<IQueryAsyncHandler<QuerySomething, string>>(() => queryHandler, Lifestyle.Singleton);

                var containerAdapter = new SimpleInjectorContainerAdapter(container);
                var resolver = new ContainerQueryAsyncHandlerResolver(containerAdapter); // Async handler resolver

                const string data = nameof(ShouldResolveQueryHandlersFromContainer);

                QueryHandlerDelegate<string> queryHandlerDelegate = resolver.ResolveQueryHandler<QuerySomething, string>();

                queryHandlerDelegate.Should().NotBeNull();

                var registeredQueryHandler = container.GetInstance<IQueryAsyncHandler<QuerySomething, string>>();

                var query = new QuerySomething(data);

                var result1 = await queryHandlerDelegate.Invoke(query);
                var result2 = await registeredQueryHandler.HandleAsync(query);

                result1.Should().Be(data);
                result2.Should().Be(data);
                result1.Should().Be(result1);
            }
        }

        #endregion Register Method
    }
}
