using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.AttributeHandlers;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Mocks.QueryHandlers
{
    public class TestAttributedQueryHandler
    {
        private readonly ITestOutputHelper _outputHelper;
        private static int _instanceCounter = 0;

        public TestAttributedQueryHandler(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _instanceCounter++;
        }

        [QueryHandler]
        public string QuerySomething(QuerySomething query)
        {
            _outputHelper.WriteLine(query.Data);
            _outputHelper.WriteLine($"Instance #{_instanceCounter}");

            return query.Data;
        }

        [QueryHandler]
        public string QuerySomethingWithException(QuerySomethingWithException query)
        {
            _outputHelper.WriteLine(query.Data);
            _outputHelper.WriteLine($"Instance #{_instanceCounter}");

            throw new NotImplementedException("This will fail.");
        }

        [QueryHandler]
        public int QuerySomething(QuerySomethingNonReferenceType query)
        {
            _outputHelper.WriteLine(query.Data.ToString());

            return query.Data;
        }

        [QueryHandler]
        public Task<string> QuerySomethingAsync(QuerySomethingAsync query)
        {
            _outputHelper.WriteLine(query.Data);

            return Task.FromResult(query.Data);
        }

        [QueryHandler]
        public Task<string> QuerySomethingAsync(QuerySomethingAsyncWithDelay query, CancellationToken cancellationToken)
        {
            _outputHelper.WriteLine(query.Data);

            return Task.FromResult(query.Data);
        }
    }

    public class TestAttributedQueryHandlerNoReturnType
    {
        private readonly ITestOutputHelper _outputHelper;

        public TestAttributedQueryHandlerNoReturnType(ITestOutputHelper testOutputHelper)
        {
            _outputHelper = testOutputHelper;
        }

        [QueryHandler]
        public void ThisShouldNotBeAllowed(QuerySomething query)
        {
            _outputHelper.WriteLine(query.Data);
        }
    }
}
