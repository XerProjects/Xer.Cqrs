using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Mocks.QueryHandlers
{
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

            return query.Data;
        }

        public int Handle(QuerySomethingNonReferenceType query)
        {
            handle(query);

            return query.Data;
        }

        public string Handle(QuerySomethingAsync query)
        {
            handle(query);

            return query.Data;
        }

        public string Handle(QuerySomethingWithException query)
        {
            handle(query);

            throw new NotImplementedException("This will fail.");
        }

        public Task<string> HandleAsync(QuerySomething query, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(query);

            return Task.FromResult(query.Data);
        }

        public Task<int> HandleAsync(QuerySomethingNonReferenceType query, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(query);

            return Task.FromResult(query.Data);
        }

        public Task<string> HandleAsync(QuerySomethingAsync query, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(query);

            return Task.FromResult(query.Data);
        }

        public async Task<string> HandleAsync(QuerySomethingAsyncWithDelay query, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(query);

            await Task.Delay(query.DelayInMilliseconds, cancellationToken);

            return query.Data;
        }

        public Task<string> HandleAsync(QuerySomethingWithException query, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(query);

            throw new NotImplementedException("This will fail.");
        }

        private void handle<TResult>(IQuery<TResult> query)
        {
            _output.WriteLine($"Executed query of type {query.GetType()} on {DateTime.Now}");
        }
    }
}
