using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xer.Cqrs.Tests.Mocks.QueryHandlers
{
    public class TestQueryHandler : IQueryHandler<QuerySomething, string>,
                                    IQueryHandler<QuerySomethingInteger, int>,
                                    IQueryHandler<QuerySomethingAsync, string>,
                                    IQueryAsyncHandler<QuerySomething, string>,
                                    IQueryAsyncHandler<QuerySomethingInteger, int>,
                                    IQueryAsyncHandler<QuerySomethingAsync, string>,
                                    IQueryAsyncHandler<QuerySomethingAsyncWithDelay, string>
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

        public int Handle(QuerySomethingInteger query)
        {
            handle(query);

            return query.Data;
        }

        public string Handle(QuerySomethingAsync query)
        {
            handle(query);

            return query.Data;
        }

        public Task<string> HandleAsync(QuerySomething query, CancellationToken cancellationToken = default(CancellationToken))
        {
            handle(query);

            return Task.FromResult(query.Data);
        }

        public Task<int> HandleAsync(QuerySomethingInteger query, CancellationToken cancellationToken = default(CancellationToken))
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

        private void handle<TResult>(IQuery<TResult> query)
        {
            _output.WriteLine($"Executed query of type {query.GetType()} on {DateTime.Now}");
        }
    }
}
