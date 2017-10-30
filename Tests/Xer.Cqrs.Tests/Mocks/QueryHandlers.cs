using System;
using System.Threading;
using System.Threading.Tasks;
using Xer.Cqrs.QueryStack;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Mocks
{
    #region Query Handlers

    public class TestQueryHandler : IQueryHandler<QuerySomething, string>,
                                    IQueryHandler<QuerySomethingNonReferenceType, int>,
                                    IQueryHandler<QuerySomethingAsync, string>,
                                    IQueryHandler<QuerySomethingWithException, string>,
                                    IQueryAsyncHandler<QuerySomething, string>,
                                    IQueryAsyncHandler<QuerySomethingNonReferenceType, int>,
                                    IQueryAsyncHandler<QuerySomethingAsync, string>,
                                    IQueryAsyncHandler<QuerySomethingAsyncWithDelay, string>,
                                    IQueryAsyncHandler<QuerySomethingWithException, string>
    {
        private readonly ITestOutputHelper _output;

        public TestQueryHandler(ITestOutputHelper output)
        {
            _output = output;
        }

        public string Handle(QuerySomething query)
        {
            handle(query);
            _output.WriteLine($"Query result:{query.Data}.");

            return query.Data;
        }

        public int Handle(QuerySomethingNonReferenceType query)
        {
            handle(query);
            _output.WriteLine($"Query result:{query.Data}.");

            return query.Data;
        }

        public string Handle(QuerySomethingAsync query)
        {
            handle(query);
            _output.WriteLine($"Query result:{query.Data}.");

            return query.Data;
        }

        public string Handle(QuerySomethingWithException query)
        {
            handle(query);
            _output.WriteLine($"Query result:{query.Data}.");

            throw new NotImplementedException("This will fail.");
        }

        public Task<string> HandleAsync(QuerySomething query, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(query);
            _output.WriteLine($"Query result:{query.Data}.");

            return Task.FromResult(query.Data);
        }

        public Task<int> HandleAsync(QuerySomethingNonReferenceType query, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(query);
            _output.WriteLine($"Query result:{query.Data}.");

            return Task.FromResult(query.Data);
        }

        public Task<string> HandleAsync(QuerySomethingAsync query, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(query);
            _output.WriteLine($"Query result:{query.Data}.");

            return Task.FromResult(query.Data);
        }

        public async Task<string> HandleAsync(QuerySomethingAsyncWithDelay query, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(query);

            await Task.Delay(query.DelayInMilliseconds, cancellationToken);

            _output.WriteLine($"Query result:{query.Data}.");

            return query.Data;
        }

        public Task<string> HandleAsync(QuerySomethingWithException query, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(query);

            throw new NotImplementedException("This will fail.");
        }

        private void handle<TResult>(IQuery<TResult> query)
        {
            _output.WriteLine($"Executed query of type {query.GetType()} on {DateTime.Now}.");
        }
    }

    #endregion Query Handlers

    #region Attributed Query Handlers

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

    #endregion Attributed Query Handlers
}
