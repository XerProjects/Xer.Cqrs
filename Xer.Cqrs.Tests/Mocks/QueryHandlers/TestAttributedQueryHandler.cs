using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.AttributeHandlers;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Mocks.QueryHandlers
{
    public class TestAttributedQueryHandler
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private static int _instanceCounter = 0;

        public TestAttributedQueryHandler(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _instanceCounter++;
        }

        [QueryHandler]
        public string QuerySomething(QuerySomething query)
        {
            _testOutputHelper.WriteLine(query.Data);
            _testOutputHelper.WriteLine($"Instance #{_instanceCounter}");

            return query.Data;
        }

        [QueryHandler]
        public int QuerySomething(QuerySomethingInteger query)
        {
            _testOutputHelper.WriteLine(query.Data.ToString());

            return query.Data;
        }

        //[QueryHandler]
        //public Task<string> QuerySomethingAsync(QuerySomething querySomething)
        //{
        //    _testOutputHelper.WriteLine(querySomething.InputMessage);

        //    return Task.FromResult(querySomething.InputMessage);
        //}

        [QueryHandler]
        public Task<string> QuerySomethingAsync(QuerySomethingAsync query)
        {
            _testOutputHelper.WriteLine(query.Data);

            return Task.FromResult(query.Data);
        }

        [QueryHandler]
        public Task<string> QuerySomethingAsync(QuerySomethingAsyncWithDelay query, CancellationToken cancellationToken)
        {
            _testOutputHelper.WriteLine(query.Data);

            return Task.FromResult(query.Data);
        }
    }
}
